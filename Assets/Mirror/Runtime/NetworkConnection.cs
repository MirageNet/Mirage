using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// A High level network connection. Responsible for sending and receiving messages
    /// </summary>
    /// <remarks>
    /// <para>A NetworkConnection corresponds to a specific connection for a host in the transport layer. It has a connectionId that is assigned by the transport layer and passed to the Initialize function.</para>
    /// <para>A NetworkClient has one NetworkConnection. A NetworkServerSimple manages multiple NetworkConnections. The NetworkServer has multiple "remote" connections and a "local" connection for the local client.</para>
    /// <para>The NetworkConnection class provides message sending and handling facilities. For sending data over a network, there are methods to send message objects, byte arrays, and NetworkWriter objects. To handle data arriving from the network, handler functions can be registered for message Ids, byte arrays can be processed by HandleBytes(), and NetworkReader object can be processed by HandleReader().</para>
    /// <para>NetworkConnection objects also act as observers for networked objects. When a connection is an observer of a networked object with a NetworkIdentity, then the object will be visible to corresponding client for the connection, and incremental state changes will be sent to the client.</para>
    /// <para>There are many virtual functions on NetworkConnection that allow its behaviour to be customized. NetworkClient and NetworkServer can both be made to instantiate custom classes derived from NetworkConnection by setting their networkConnectionClass member variable.</para>
    /// </remarks>
    public abstract class NetworkConnection
    {
        #region Properties

        // internal so it can be tested
        internal readonly HashSet<NetworkIdentity> visList = new HashSet<NetworkIdentity>();

        private readonly Dictionary<int, NetworkMessageDelegate> messageHandlers = new Dictionary<int, NetworkMessageDelegate>();

        /// <summary>
        /// Transport connection
        /// </summary>
        public IConnection Connection { get; }

        /// <summary>
        /// Flag that indicates the client has been authenticated.
        /// </summary>
        public bool isAuthenticated;

        /// <summary>
        /// General purpose object to hold authentication data, character selection, tokens, etc.
        /// associated with the connection for reference after Authentication completes.
        /// </summary>
        public object authenticationData;

        /// <summary>
        /// Flag that tells if the connection has been marked as "ready" by a client calling ClientScene.Ready().
        /// <para>This property is read-only. It is set by the system on the client when ClientScene.Ready() is called, and set by the system on the server when a ready message is received from a client.</para>
        /// <para>A client that is ready is sent spawned objects by the server and updates to the state of spawned objects. A client that is not ready is not sent spawned objects.</para>
        /// </summary>
        public bool isReady;

        /// <summary>
        /// The IP address / URL / FQDN associated with the connection.
        /// Can be useful for a game master to do IP Bans etc.
        /// </summary>
        public virtual EndPoint address => Connection.GetEndPointAddress();

        /// <summary>
        /// The last time that a message was received on this connection.
        /// <para>This includes internal system messages (such as Commands and ClientRpc calls) and user messages.</para>
        /// </summary>
        public float lastMessageTime;

        /// <summary>
        /// The NetworkIdentity for this connection.
        /// </summary>
        public NetworkIdentity identity { get; internal set; }

        /// <summary>
        /// A list of the NetworkIdentity objects owned by this connection. This list is read-only.
        /// <para>This includes the player object for the connection - if it has localPlayerAutority set, and any objects spawned with local authority or set with AssignLocalAuthority.</para>
        /// <para>This list can be used to validate messages from clients, to ensure that clients are only trying to control objects that they own.</para>
        /// </summary>
        // IMPORTANT: this needs to be <NetworkIdentity>, not <uint netId>. fixes a bug where DestroyOwnedObjects wouldn't find
        //            the netId anymore: https://github.com/vis2k/Mirror/issues/1380 . Works fine with NetworkIdentity pointers though.
        public readonly HashSet<NetworkIdentity> clientOwnedObjects = new HashSet<NetworkIdentity>();

        /// <summary>
        /// Setting this to true will log the contents of network message to the console.
        /// </summary>
        /// <remarks>
        /// <para>Warning: this can be a lot of data and can be very slow. Both incoming and outgoing messages are logged. The format of the logs is:</para>
        /// <para>ConnectionSend con:1 bytes:11 msgId:5 FB59D743FD120000000000 ConnectionRecv con:1 bytes:27 msgId:8 14F21000000000016800AC3FE090C240437846403CDDC0BD3B0000</para>
        /// <para>Note that these are application-level network messages, not protocol-level packets. There will typically be multiple network messages combined in a single protocol packet.</para>
        /// </remarks>
        public bool logNetworkMessages;

        #endregion

        /// <summary>
        /// Creates a new NetworkConnection with the specified address
        /// </summary>
        internal NetworkConnection(IConnection connection)
        {
            this.Connection = connection;
        }


        #region Receiving Messages


        private readonly MemoryStream buffer = new MemoryStream();
        /// <summary>
        /// Receives a message from the transport and dispatches it to the handler
        /// </summary>
        /// <returns>false if the connection is closed </returns>
        internal async Task<bool> DispatchMessage()
        {
            buffer.SetLength(0);

            if (await Connection.ReceiveAsync(buffer))
            {
                buffer.TryGetBuffer(out ArraySegment<byte> data);
                TransportReceive(data, Channels.DefaultReliable);
                return true;
            }
            return false;
        }


        private void InvokeHandler(int msgType, NetworkReader reader, int channelId)
        {
            if (messageHandlers.TryGetValue(msgType, out NetworkMessageDelegate msgDelegate))
                msgDelegate(this, reader, channelId);
            else
                throw new KeyNotFoundException($"Unknown message ID {msgType} {this}");
        }

        /// <summary>
        /// This function invokes the registered handler function for a message.
        /// <para>Network connections used by the NetworkClient and NetworkServer use this function for handling network messages.</para>
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="msg">The message object to process.</param>
        /// <returns></returns>
        public void InvokeHandler<T>(T msg, int channelId) where T : IMessageBase
        {
            // get writer from pool
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // if it is a value type,  just use typeof(T) to avoid boxing
                // this works because value types cannot be derived
                // if it is a reference type (for example IMessageBase),
                // ask the message for the real type
                int msgType = MessagePacker.GetId(typeof(T).IsValueType ? typeof(T) : msg.GetType());

                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(segment))
                    InvokeHandler(msgType, networkReader, channelId);
            }
        }

        // note: original HLAPI HandleBytes function handled >1 message in a while loop, but this wasn't necessary
        //       anymore because NetworkServer/NetworkClient Update both use while loops to handle >1 data events per
        //       frame already.
        //       -> in other words, we always receive 1 message per Receive call, never two.
        //       -> can be tested easily with a 1000ms send delay and then logging amount received in while loops here
        //          and in NetworkServer/Client Update. HandleBytes already takes exactly one.
        /// <summary>
        /// This function allows custom network connection classes to process data from the network before it is passed to the application.
        /// </summary>
        /// <param name="buffer">The data received.</param>
        internal void TransportReceive(ArraySegment<byte> buffer, int channelId)
        {
            // unpack message
            using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(buffer))
            {
                int msgType = MessagePacker.UnpackId(networkReader);
                // logging
                if (logNetworkMessages) Debug.Log("ConnectionRecv " + this + " msgType:" + msgType + " content:" + BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count));

                // try to invoke the handler for that message
                InvokeHandler(msgType, networkReader, channelId);
            }
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<C, T>(Action<C, T> handler, bool requireAuthentication = true)
             where T : IMessageBase, new()
            where C : NetworkConnection
        {
            void handlerWrapper(NetworkConnection conn, NetworkReader reader, int channelId)
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
                T message = default;
                try
                {
                    if (requireAuthentication && !conn.isAuthenticated)
                    {
                        // message requires authentication, but the connection was not authenticated
                        throw new UnauthorizedAccessException($"Closing connection: {conn}. Received message {typeof(T)} that required authentication, but the user has not authenticated yet");
                    }

                    message = typeof(T).IsValueType ? default(T) : new T();
                    message.Deserialize(reader);

                    handler((C)conn, message);
                }
                finally
                {
                    NetworkDiagnostics.OnReceive(message, channelId, reader.Length);
                }

            }

            int msgType = MessagePacker.GetId<T>();

            if (messageHandlers.ContainsKey(msgType))
            {
                if (LogFilter.Debug) Debug.Log("NetworkServer.RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = handlerWrapper;
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="handler"></param>
        /// <param name="requireAuthentication">true if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : IMessageBase, new()
        {
            RegisterHandler((NetworkConnectionToServer _, T value) => { handler(value); }, requireAuthentication);
        }

        #endregion


        /// <summary>
        /// Disconnects this connection.
        /// </summary>
        public virtual void Disconnect()
        {
            // set not ready and handle clientscene disconnect in any case
            // (might be client or host mode here)
            isReady = false;
            RemoveObservers();
            Connection.Disconnect();
        }

        /// <summary>
        /// This sends a network message with a message ID on the connection. This message is sent on channel zero, which by default is the reliable channel.
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        /// <returns></returns>
        public void Send<T>(T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message and send allocation free
                MessagePacker.Pack(msg, writer);
                NetworkDiagnostics.OnSend(msg, channelId, writer.Position, 1);
                _ = SendAsync(writer.ToArraySegment(), channelId);
            }
        }

        protected virtual Task SendAsync(ArraySegment<byte> segment, int channelId = Channels.DefaultReliable)
        {
            if (logNetworkMessages) Debug.Log("ConnectionSend " + this + " bytes:" + BitConverter.ToString(segment.Array, segment.Offset, segment.Count));

            return Connection.SendAsync(segment);
        }

        public static void Send<T>(IEnumerable<NetworkConnection> connections, T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message and send allocation free
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();

                int count = 0;
                foreach (NetworkConnection connection in connections)
                {
                    _ = connection.SendAsync(segment, channelId);
                    count++;
                }

                NetworkDiagnostics.OnSend(msg, channelId, segment.Count, count);
            }
        }

        internal void AddToVisList(NetworkIdentity identity)
        {
            visList.Add(identity);

            // spawn identity for this conn
            identity.server.ShowForConnection(identity, this);
        }

        internal void RemoveFromVisList(NetworkIdentity identity, bool isDestroyed)
        {
            visList.Remove(identity);

            if (!isDestroyed)
            {
                // hide identity for this conn
                identity.server.HideForConnection(identity, this);
            }
        }

        internal void RemoveObservers()
        {
            foreach (NetworkIdentity identity in visList)
            {
                identity.RemoveObserverInternal(this);
            }
            visList.Clear();
        }


        internal void AddOwnedObject(NetworkIdentity obj)
        {
            clientOwnedObjects.Add(obj);
        }

        internal void RemoveOwnedObject(NetworkIdentity obj)
        {
            clientOwnedObjects.Remove(obj);
        }

        internal void DestroyOwnedObjects()
        {
            // create a copy because the list might be modified when destroying
            var tmp = new HashSet<NetworkIdentity>(clientOwnedObjects);
            foreach (NetworkIdentity netIdentity in tmp)
            {
                if (netIdentity != null)
                {
                    identity.server.Destroy(netIdentity.gameObject);
                }
            }

            // clear the hashset because we destroyed them all
            clientOwnedObjects.Clear();
        }
    }
}
