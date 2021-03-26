using System;
using System.Collections.Generic;
using System.IO;
using Mirage.Logging;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Sends and handles messages
    /// </summary>
    public sealed class MessageBroker : IMessageHandler, IMessageSender, IMessageReceiver, INotifySender, INotifyReceiver, IDataHandler
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(MessageBroker));

        // Handles network messages on client and server
        internal delegate void NetworkMessageDelegate(IConnectionPlayer player, NetworkReader reader, int channelId);

        // message handlers for this connection
        internal readonly Dictionary<int, NetworkMessageDelegate> messageHandlers = new Dictionary<int, NetworkMessageDelegate>();

        public MessageBroker()
        {
            lastNotifySentTime = Time.unscaledTime;

            // a black message to ensure a notify timeout
            RegisterHandler<NotifyAck>(msg => { });
        }

        #region Receive

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<NetworkPlayer, T> handler)
        {
            RegisterHandler((IConnectionPlayer connPlayer, T value) => { handler(connPlayer as NetworkPlayer, value); });
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
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<IConnectionPlayer, T> handler)
        {
            int msgType = MessagePacker.GetId<T>();
            if (logger.filterLogType == LogType.Log && messageHandlers.ContainsKey(msgType))
            {
                logger.Log("NetworkServer.RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = MessageHandler(handler);
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


        private static NetworkMessageDelegate MessageHandler<T>(Action<IConnectionPlayer, T> handler)
        {
            void AdapterFunction(IConnectionPlayer player, NetworkReader reader, int channelId)
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
                try
                {
                    message = reader.Read<T>();
                }
                finally
                {
                    NetworkDiagnostics.OnReceive(message, channelId, reader.Length);
                }

                handler(player, message);
            }
            return AdapterFunction;
        }


        // use explict interface to hide at high level
        void IDataHandler.ReceiveData(IConnectionPlayer player, ArraySegment<byte> segment)
        {
            // unpack message
            using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(segment))
            {
                try
                {
                    int msgType = MessagePacker.UnpackId(networkReader);

                    if (msgType == MessagePacker.GetId<NotifyPacket>())
                    {
                        // this is a notify message, send to the notify receive
                        NotifyPacket notifyPacket = networkReader.ReadNotifyPacket();
                        ReceiveNotify(player, notifyPacket, networkReader);
                    }
                    else
                    {
                        // try to invoke the handler for that message
                        InvokeHandler(player, msgType, networkReader);
                    }
                }
                catch (InvalidDataException ex)
                {
                    logger.Log(ex.ToString());
                }
                catch (Exception ex)
                {
                    logger.LogError("Closed connection: " + this + ". Invalid message " + ex);
                    player?.Connection?.DisconnectPlayer(player);
                }
            }
        }

        internal void InvokeHandler(IConnectionPlayer player, int msgType, NetworkReader reader, int channelId = default)
        {
            if (messageHandlers.TryGetValue(msgType, out NetworkMessageDelegate msgDelegate))
            {
                msgDelegate(player, reader, channelId);
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
        #endregion

        #region Send

        /// <summary>
        /// This sends a network message to the connection.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        /// <returns></returns>
        public void Send<T>(Connection connection, T message, int channelId = Channel.Reliable)
        {
            if (connection == null)
            {
                // todo invoke locally when sending to player without connecton
                logger.LogError("can't send to player without connection");
                return;
            }

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message and send allocation free
                MessagePacker.Pack(message, writer);
                NetworkDiagnostics.OnSend(message, channelId, writer.Length, 1);
                Send(connection, writer.ToArraySegment(), channelId);
            }
        }

        // internal because no one except Mirage should send bytes directly to
        // the client. they would be detected as a message. send messages instead.
        public void Send(Connection connection, ArraySegment<byte> segment, int channelId = Channel.Reliable)
        {
            if (connection == null)
            {
                // todo invoke locally when sending to player without connecton
                logger.LogError("can't send to player without connection");
                return;
            }
            connection.Send(segment, channelId);
        }
        /// <summary>
        /// This sends a network message to the connection.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        /// <returns></returns>
        public void Send<T>(NetworkPlayer player, T message, int channelId = Channel.Reliable)
        {
            Send(player.Connection, message, channelId);
        }

        // internal because no one except Mirage should send bytes directly to
        // the client. they would be detected as a message. send messages instead.
        public void Send(NetworkPlayer player, ArraySegment<byte> segment, int channelId = Channel.Reliable)
        {
            Send(player.Connection, segment, channelId);
        }
        #endregion

        #region Notify

        internal struct PacketEnvelope
        {
            internal ushort Sequence;
            internal object Token;
        }
        const int ACK_MASK_BITS = sizeof(ulong) * 8;
        const int WINDOW_SIZE = 512;
        // packages will be acked no longer than this time
        public float NOTIFY_ACK_TIMEOUT = 0.3f;

        private Sequencer sequencer = new Sequencer(16);
        readonly Queue<PacketEnvelope> sendWindow = new Queue<PacketEnvelope>(WINDOW_SIZE);
        private float lastNotifySentTime;

        private ushort receiveSequence;
        private ulong receiveMask;



        /// <summary>
        /// Sends a message, but notify when it is delivered or lost
        /// </summary>
        /// <typeparam name="T">type of message to send</typeparam>
        /// <param name="message">message to send</param>
        /// <param name="token">a arbitrary object that the sender will receive with their notification</param>
        public void SendNotify<T>(IConnectionPlayer player, T message, object token, int channelId = Channel.Unreliable)
        {
            if (sendWindow.Count == WINDOW_SIZE)
            {
                NotifyLost?.Invoke(player, token);
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
                Send(player.Connection, writer.ToArraySegment(), channelId);
                lastNotifySentTime = Time.unscaledTime;
            }

        }

        internal void ReceiveNotify(IConnectionPlayer player, NotifyPacket notifyPacket, NetworkReader networkReader, int channelId = default)
        {
            int sequenceDistance = (int)sequencer.Distance(notifyPacket.Sequence, receiveSequence);

            // sequence is so far out of bounds we can't save, just kick them
            if (Math.Abs(sequenceDistance) > WINDOW_SIZE)
            {
                player?.Connection?.DisconnectPlayer(player);
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

            AckPackets(player, notifyPacket.ReceiveSequence, notifyPacket.AckMask);

            int msgType = MessagePacker.UnpackId(networkReader);
            InvokeHandler(player, msgType, networkReader, channelId);

            if (Time.unscaledTime - lastNotifySentTime > NOTIFY_ACK_TIMEOUT)
            {
                SendNotify(player, new NotifyAck(), null, channelId);
            }
        }

        // the other end just sent us a message
        // and it told us the latest message it got
        // and the ack mask
        private void AckPackets(IConnectionPlayer player, ushort receiveSequence, ulong ackMask)
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
                    NotifyLost?.Invoke(player, envelope.Token);
                }
                else
                {
                    NotifyDelivered?.Invoke(player, envelope.Token);
                }
            }
        }


        /// <summary>
        /// Raised when a message is delivered
        /// </summary>
        public event Action<IConnectionPlayer, object> NotifyDelivered;

        /// <summary>
        /// Raised when a message is lost
        /// </summary>
        public event Action<IConnectionPlayer, object> NotifyLost;
        #endregion
    }
    public static class ConnectionChannelExtensions
    {
        public static void Send(this Connection connection, ArraySegment<byte> segment, int channel)
        {
            switch (channel)
            {
                case Channel.Reliable:
                    connection.SendReliable(segment);
                    break;
                case Channel.Unreliable:
                    connection.SendUnreiable(segment);
                    break;
            }
        }
    }
}
