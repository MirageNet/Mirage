using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mirage
{
    [NetworkMessage]
    public struct NotifyAck
    {
    }

    /// <summary>
    /// A High level network connection. This is used for connections from client-to-server and for connection from server-to-client.
    /// </summary>
    /// <remarks>
    /// <para>A NetworkConnection corresponds to a specific connection for a host in the transport layer. It has a connectionId that is assigned by the transport layer and passed to the Initialize function.</para>
    /// <para>A NetworkClient has one NetworkConnection. A NetworkServerSimple manages multiple NetworkConnections. The NetworkServer has multiple "remote" connections and a "local" connection for the local client.</para>
    /// <para>The NetworkConnection class provides message sending and handling facilities. For sending data over a network, there are methods to send message objects, byte arrays, and NetworkWriter objects. To handle data arriving from the network, handler functions can be registered for message Ids, byte arrays can be processed by HandleBytes(), and NetworkReader object can be processed by HandleReader().</para>
    /// <para>NetworkConnection objects also act as observers for networked objects. When a connection is an observer of a networked object with a NetworkIdentity, then the object will be visible to corresponding client for the connection, and incremental state changes will be sent to the client.</para>
    /// <para>There are many virtual functions on NetworkConnection that allow its behaviour to be customized. NetworkClient and NetworkServer can both be made to instantiate custom classes derived from NetworkConnection by setting their networkConnectionClass member variable.</para>
    /// </remarks>
    public sealed class NetworkPlayer : INetworkPlayer
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkPlayer));

        // Handles network messages on client and server
        internal delegate void NetworkMessageDelegate(INetworkPlayer player, NetworkReader reader, int channelId);

        // internal so it can be tested
        private readonly HashSet<NetworkIdentity> visList = new HashSet<NetworkIdentity>();

        // message handlers for this connection
        internal readonly Dictionary<int, NetworkMessageDelegate> messageHandlers = new Dictionary<int, NetworkMessageDelegate>();

        /// <summary>
        /// Transport level connection
        /// </summary>
        /// <remarks>
        /// <para>On a server, this Id is unique for every connection on the server. On a client this Id is local to the client, it is not the same as the Id on the server for this connection.</para>
        /// <para>Transport layers connections begin at one. So on a client with a single connection to a server, the connectionId of that connection will be one. In NetworkServer, the connectionId of the local connection is zero.</para>
        /// <para>Clients do not know their connectionId on the server, and do not know the connectionId of other clients on the server.</para>
        /// </remarks>
        private readonly SocketLayer.IConnection connection;

        /// <summary>
        /// Has this player been marked as disconnected
        /// <para>Messages sent to disconnected players will be ignored</para>
        /// </summary>
        bool isDisconnected = false;


        /// <summary>
        /// General purpose object to hold authentication data, character selection, tokens, etc.
        /// associated with the connection for reference after Authentication completes.
        /// </summary>
        public object AuthenticationData { get; set; }

        /// <summary>
        /// Flag that tells if the connection has been marked as "ready" by a client calling ClientScene.Ready().
        /// <para>This property is read-only. It is set by the system on the client when ClientScene.Ready() is called, and set by the system on the server when a ready message is received from a client.</para>
        /// <para>A client that is ready is sent spawned objects by the server and updates to the state of spawned objects. A client that is not ready is not sent spawned objects.</para>
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// The IP address / URL / FQDN associated with the connection.
        /// Can be useful for a game master to do IP Bans etc.
        /// </summary>
        public EndPoint Address => connection.EndPoint;

        public SocketLayer.IConnection Connection => connection;

        /// <summary>
        /// Disconnects the player.
        /// <para>A disconnected player can not send messages</para>
        /// </summary>
        /// <remarks>
        /// This method exists so that users do not need to add reference to SocketLayer asmdef
        /// </remarks>
        public void Disconnect()
        {
            connection.Disconnect();
            isDisconnected = true;
        }

        /// <summary>
        /// Marks player as disconnected, used when the disconnect call is from peer
        /// <para>A disconnected player can not send messages</para>
        /// </summary>
        public void MarkAsDisconnected()
        {
            isDisconnected = true;
        }

        /// <summary>
        /// The NetworkIdentity for this connection.
        /// </summary>
        public NetworkIdentity Identity { get; set; }

        /// <summary>
        /// A list of the NetworkIdentity objects owned by this connection. This list is read-only.
        /// <para>This includes the player object for the connection - if it has localPlayerAutority set, and any objects spawned with local authority or set with AssignLocalAuthority.</para>
        /// <para>This list can be used to validate messages from clients, to ensure that clients are only trying to control objects that they own.</para>
        /// </summary>
        // IMPORTANT: this needs to be <NetworkIdentity>, not <uint netId>. fixes a bug where DestroyOwnedObjects wouldn't find
        //            the netId anymore: https://github.com/vis2k/Mirror/issues/1380 . Works fine with NetworkIdentity pointers though.
        private readonly HashSet<NetworkIdentity> clientOwnedObjects = new HashSet<NetworkIdentity>();

        /// <summary>
        /// Creates a new NetworkConnection with the specified address and connectionId
        /// </summary>
        /// <param name="networkConnectionId"></param>
        public NetworkPlayer(SocketLayer.IConnection connection)
        {
            Assert.IsNotNull(connection);
            this.connection = connection;

            lastNotifySentTime = Time.unscaledTime;
            // a black message to ensure a notify timeout
            RegisterHandler<NotifyAck>(msg => { });
        }


        private static NetworkMessageDelegate MessageHandler<T>(Action<INetworkPlayer, T> handler)
        {
            void AdapterFunction(INetworkPlayer player, NetworkReader reader, int channelId)
            {
                // protect against DOS attacks if attackers try to send invalid
                // data packets to crash the server/client. there are a thousand
                // ways to cause an exception in data handling:
                // - invalid headers
                // - invalid message ids
                // - invalid data causing exceptions
                // - negative ReadBytesAndSize prefixes
                // - invalid utf8 strings
                // - etc.
                //
                // let's catch them all and then disconnect that connection to avoid
                // further attacks.
                var message = default(T);

                // record start position for NetworkDiagnostics because reader might contain multiple messages if using batching
                int startPos = reader.Position;
                try
                {
                    message = reader.Read<T>();
                }
                finally
                {
                    int endPos = reader.Position;
                    NetworkDiagnostics.OnReceive(message, channelId, endPos - startPos);
                }

                handler(player, message);
            }
            return AdapterFunction;
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<INetworkPlayer, T> handler)
        {
            int msgType = MessagePacker.GetId<T>();
            if (logger.filterLogType == LogType.Log && messageHandlers.ContainsKey(msgType))
            {
                logger.Log("NetworkServer.RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = MessageHandler(handler);
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<T> handler)
        {
            RegisterHandler<T>((_, value) => { handler(value); });
        }

        /// <summary>
        /// Unregisters a handler for a particular message type.
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        public void UnregisterHandler<T>()
        {
            int msgType = MessagePacker.GetId<T>();
            messageHandlers.Remove(msgType);
        }

        /// <summary>
        /// Clear all registered callback handlers.
        /// </summary>
        public void ClearHandlers()
        {
            messageHandlers.Clear();
        }

        /// <summary>
        /// This sends a network message to the connection.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        /// <returns></returns>
        public void Send<T>(T message, int channelId = Channel.Reliable)
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message and send allocation free
                MessagePacker.Pack(message, writer);
                NetworkDiagnostics.OnSend(message, channelId, writer.Length, 1);
                Send(writer.ToArraySegment(), channelId);
            }
        }

        // internal because no one except Mirage should send bytes directly to
        // the client. they would be detected as a message. send messages instead.
        public void Send(ArraySegment<byte> segment, int channelId = Channel.Reliable)
        {
            if (isDisconnected) { return; }

            // todo use buffer pool
            byte[] packet = segment.ToArray();
            if (channelId == Channel.Reliable)
            {
                connection.SendReliable(packet);
            }
            else
            {
                connection.SendUnreliable(packet);
            }
        }


        public override string ToString()
        {
            return $"connection({Address})";
        }

        public void AddToVisList(NetworkIdentity identity)
        {
            visList.Add(identity);
        }

        public void RemoveFromVisList(NetworkIdentity identity)
        {
            visList.Remove(identity);
        }

        public void RemoveObservers()
        {
            foreach (NetworkIdentity identity in visList)
            {
                identity.RemoveObserverInternal(this);
            }
            visList.Clear();
        }

        internal void InvokeHandler(int msgType, NetworkReader reader, int channelId)
        {
            if (messageHandlers.TryGetValue(msgType, out NetworkMessageDelegate msgDelegate))
            {
                msgDelegate(this, reader, channelId);
            }
            else
            {
                try
                {
                    Type type = MessagePacker.GetMessageType(msgType);
                    throw new InvalidDataException($"Unexpected message {type} received in {this}. Did you register a handler for it?");
                }
                catch (KeyNotFoundException)
                {
                    throw new InvalidDataException($"Unexpected message ID {msgType} received in {this}. May be due to no existing RegisterHandler for this message.");
                }
            }
        }

        void IMessageHandler.HandleMessage(ArraySegment<byte> packet)
        {
            // unpack message
            using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(packet))
            {
                try
                {
                    int msgType = MessagePacker.UnpackId(networkReader);

                    // todo remove channel from handler
                    const int channelId = 0;
                    InvokeHandler(msgType, networkReader, channelId);
                }
                catch (InvalidDataException ex)
                {
                    logger.Log(ex.ToString());
                }
                catch (Exception ex)
                {
                    logger.LogError("Closed connection: " + this + ". Invalid message " + ex);
                    Connection?.Disconnect();
                }
            }
        }

        public void AddOwnedObject(NetworkIdentity networkIdentity)
        {
            clientOwnedObjects.Add(networkIdentity);
        }

        public void RemoveOwnedObject(NetworkIdentity networkIdentity)
        {
            clientOwnedObjects.Remove(networkIdentity);
        }

        public void DestroyOwnedObjects()
        {
            // create a copy because the list might be modified when destroying
            var tmp = new HashSet<NetworkIdentity>(clientOwnedObjects);
            foreach (NetworkIdentity netIdentity in tmp)
            {
                //dont destroy self yet.
                if (netIdentity != null && netIdentity != Identity && Identity.ServerObjectManager != null)
                {
                    Identity.ServerObjectManager.Destroy(netIdentity.gameObject);
                }
            }

            if (Identity != null && Identity.Server != null)
                // Destroy the connections own identity.
                Identity.ServerObjectManager.Destroy(Identity.gameObject);

            // clear the hashset because we destroyed them all
            clientOwnedObjects.Clear();
        }

        #region Notify

        internal struct PacketEnvelope
        {
            internal ushort Sequence;
            internal object Token;
        }
        const int ACK_MASK_BITS = sizeof(ulong) * 8;
        const int WINDOW_SIZE = 512;
        // packages will be acked no longer than this time
        [Obsolete("Use Peer instead", true)]
        public float NOTIFY_ACK_TIMEOUT = 0.3f;

        private Sequencer sequencer = new Sequencer(16);
        readonly Queue<PacketEnvelope> sendWindow = new Queue<PacketEnvelope>(WINDOW_SIZE);
        private float lastNotifySentTime;

        // the first sequence will be 0,  so
        // we need the last received sequence to be just before that
        // this is unsigned and wraps,  so 0 - 1 == ushort.MaxValue
        private ushort receiveSequence = ushort.MaxValue;
        private ulong receiveMask;



        /// <summary>
        /// Sends a message, but notify when it is delivered or lost
        /// </summary>
        /// <typeparam name="T">type of message to send</typeparam>
        /// <param name="message">message to send</param>
        /// <param name="token">a arbitrary object that the sender will receive with their notification</param>
        [Obsolete("Use Peer instead", true)]
        public void SendNotify<T>(T message, object token, int channelId = Channel.Unreliable)
        {
            if (sendWindow.Count == WINDOW_SIZE)
            {
                NotifyLost?.Invoke(this, token);
                return;
            }

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                var notifyPacket = new NotifyPacket
                {
                    Sequence = (ushort)sequencer.Next(),
                    ReceiveSequence = receiveSequence,
                    AckMask = receiveMask
                };

                sendWindow.Enqueue(new PacketEnvelope
                {
                    Sequence = notifyPacket.Sequence,
                    Token = token
                });

                MessagePacker.Pack(notifyPacket, writer);
                MessagePacker.Pack(message, writer);
                NetworkDiagnostics.OnSend(message, channelId, writer.Length, 1);
                Send(writer.ToArraySegment(), channelId);
                lastNotifySentTime = Time.unscaledTime;
            }

        }

        [Obsolete("Use Peer instead", true)]
        internal void ReceiveNotify(NotifyPacket notifyPacket, NetworkReader networkReader, int channelId)
        {
            int sequenceDistance = (int)sequencer.Distance(notifyPacket.Sequence, receiveSequence);

            // sequence is so far out of bounds we can't save, just kick him (or her!)
            if (Math.Abs(sequenceDistance) > WINDOW_SIZE)
            {
                connection?.Disconnect();
                return;
            }

            // this message is old,  we already received
            // a newer or duplicate packet.  Discard it
            if (sequenceDistance <= 0)
                return;

            receiveSequence = notifyPacket.Sequence;

            if (sequenceDistance >= ACK_MASK_BITS)
                receiveMask = 1;
            else
                receiveMask = (receiveMask << sequenceDistance) | 1;

            AckPackets(notifyPacket.ReceiveSequence, notifyPacket.AckMask);

            int msgType = MessagePacker.UnpackId(networkReader);
            InvokeHandler(msgType, networkReader, channelId);

            if (Time.unscaledTime - lastNotifySentTime > NOTIFY_ACK_TIMEOUT)
            {
                SendNotify(new NotifyAck(), null, channelId);
            }
        }

        // the other end just sent us a message
        // and it told us the latest message it got
        // and the ack mask
        [Obsolete("Use Peer instead", true)]
        private void AckPackets(ushort receiveSequence, ulong ackMask)
        {
            while (sendWindow.Count > 0)
            {
                PacketEnvelope envelope = sendWindow.Peek();

                int distance = (int)sequencer.Distance(envelope.Sequence, receiveSequence);

                if (distance > 0)
                    break;

                sendWindow.Dequeue();

                // if any of these cases trigger, packet is most likely lost
                if ((distance <= -ACK_MASK_BITS) || ((ackMask & (1UL << -distance)) == 0UL))
                {
                    NotifyLost?.Invoke(this, envelope.Token);
                }
                else
                {
                    NotifyDelivered?.Invoke(this, envelope.Token);
                }
            }
        }

        /// <summary>
        /// Raised when a message is delivered
        /// </summary>
        [Obsolete("Use Peer instead", true)]
        public event Action<INetworkPlayer, object> NotifyDelivered;

        /// <summary>
        /// Raised when a message is lost
        /// </summary>
        [Obsolete("Use Peer instead", true)]
        public event Action<INetworkPlayer, object> NotifyLost;
        #endregion
    }
}
