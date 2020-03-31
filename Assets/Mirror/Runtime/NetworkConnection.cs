using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace Mirror
{
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
    public abstract class NetworkConnection : IDisposable
    {
        // Handles network messages on client and server
        private delegate void NetworkMessageDelegate(NetworkConnection conn, NetworkReader reader, int channelId);


        // internal so it can be tested
        internal readonly HashSet<NetworkIdentity> visList = new HashSet<NetworkIdentity>();

        // message handlers for this connection
        private readonly Dictionary<int, NetworkMessageDelegate> messageHandlers = new Dictionary<int, NetworkMessageDelegate>();

        /// <summary>
        /// Unique identifier for this connection that is assigned by the transport layer.
        /// </summary>
        /// <remarks>
        /// <para>On a server, this Id is unique for every connection on the server. On a client this Id is local to the client, it is not the same as the Id on the server for this connection.</para>
        /// <para>Transport layers connections begin at one. So on a client with a single connection to a server, the connectionId of that connection will be one. In NetworkServer, the connectionId of the local connection is zero.</para>
        /// <para>Clients do not know their connectionId on the server, and do not know the connectionId of other clients on the server.</para>
        /// </remarks>
        public readonly int connectionId;

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
        public abstract EndPoint Address { get; }

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

        /// <summary>
        /// Creates a new NetworkConnection with the specified address
        /// </summary>
        internal NetworkConnection()
        {
        }

        /// <summary>
        /// Creates a new NetworkConnection with the specified address and connectionId
        /// </summary>
        /// <param name="networkConnectionId"></param>
        internal NetworkConnection(int networkConnectionId)
        {
            connectionId = networkConnectionId;
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of this connection, releasing channel buffers that it holds.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            clientOwnedObjects.Clear();
        }

        /// <summary>
        /// Disconnects this connection.
        /// </summary>
        public abstract void Disconnect();

        private static NetworkMessageDelegate MessageHandler<T, C>(Action<C, T> handler, bool requireAuthenication)
            where T : IMessageBase, new()
            where C : NetworkConnection
        {
            void AdapterFunction(NetworkConnection conn, NetworkReader reader, int channelId)
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
                    if (requireAuthenication && !conn.isAuthenticated)
                    {
                        // message requires authentication, but the connection was not authenticated
                        Debug.LogWarning($"Closing connection: {conn}. Received message {typeof(T)} that required authentication, but the user has not authenticated yet");
                        conn.Disconnect();
                        return;
                    }

                    message = default(T) != null ? default(T) : new T();
                    message.Deserialize(reader);
                }
                catch (Exception exception)
                {
                    Debug.LogError("Closed connection: " + conn + ". This can happen if the other side accidentally (or an attacker intentionally) sent invalid data. Reason: " + exception);
                    conn.Disconnect();
                    return;
                }
                finally
                {
                    NetworkDiagnostics.OnReceive(message, channelId, reader.Length);
                }

                handler((C)conn, message);
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
        public void RegisterHandler<C, T>(Action<C, T> handler, bool requireAuthentication = true)
            where T : IMessageBase, new()
            where C : NetworkConnection
        {
            int msgType = MessagePacker.GetId<T>();
            if (LogFilter.Debug && messageHandlers.ContainsKey(msgType))
            {
                Debug.Log("NetworkServer.RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = MessageHandler(handler, requireAuthentication);
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : IMessageBase, new()
        {
            RegisterHandler<NetworkConnection, T>((_, value) => { handler(value); }, requireAuthentication);
        }

        /// <summary>
        /// Unregisters a handler for a particular message type.
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        public void UnregisterHandler<T>() where T : IMessageBase
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
        /// This sends a network message with a message ID on the connection. This message is sent on channel zero, which by default is the reliable channel.
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        /// <returns></returns>
        public bool Send<T>(T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message and send allocation free
                MessagePacker.Pack(msg, writer);
                NetworkDiagnostics.OnSend(msg, channelId, writer.Position, 1);
                return Send(writer.ToArraySegment(), channelId);
            }
        }

        // internal because no one except Mirror should send bytes directly to
        // the client. they would be detected as a message. send messages instead.
        internal abstract bool Send(ArraySegment<byte> segment, int channelId = Channels.DefaultReliable);

        // cache the Send(connectionIds) list to avoid allocating each time
        static readonly List<int> connectionIdsCache = new List<int>();

        public static bool Send<T>(IEnumerable<NetworkConnection> connections, T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();

                // filter and then send to all internet connections at once
                // -> makes code more complicated, but is HIGHLY worth it to
                //    avoid allocations, allow for multicast, etc.
                connectionIdsCache.Clear();
                bool result = true;
                int count = 0;

                foreach (NetworkConnection connection in connections)
                {
                    // use local connection directly because it doesn't send via transport
                    if (connection is ULocalConnectionToClient)
                        result &= connection.Send(segment);
                    // gather all internet connections
                    else
                        connectionIdsCache.Add(connection.connectionId);

                    count++;
                }

                // send to all internet connections at once
                if (connectionIdsCache.Count > 0)
                {
                    result &= NetworkConnectionToClient.Send(connectionIdsCache, segment, channelId);
                }

                NetworkDiagnostics.OnSend(msg, channelId, segment.Count, count);

                return result;
            }
        }

        public override string ToString()
        {
            return $"connection({connectionId})";
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

        internal bool InvokeHandler(int msgType, NetworkReader reader, int channelId)
        {
            if (messageHandlers.TryGetValue(msgType, out NetworkMessageDelegate msgDelegate))
            {
                msgDelegate(this, reader, channelId);
                return true;
            }
            if (Debug.isDebugBuild) Debug.Log("Unknown message ID " + msgType + " " + this + ". May be due to no existing RegisterHandler for this message.");
            return false;
        }

        /// <summary>
        /// This function invokes the registered handler function for a message.
        /// <para>Network connections used by the NetworkClient and NetworkServer use this function for handling network messages.</para>
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="msg">The message object to process.</param>
        /// <returns></returns>
        public bool InvokeHandler<T>(T msg, int channelId) where T : IMessageBase
        {
            // get writer from pool
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // if it is a value type,  just use typeof(T) to avoid boxing
                // this works because value types cannot be derived
                // if it is a reference type (for example IMessageBase),
                // ask the message for the real type
                int msgType = MessagePacker.GetId(default(T) != null ? typeof(T) : msg.GetType());

                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(segment))
                    return InvokeHandler(msgType, networkReader, channelId);
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
                try
                {
                    int msgType = MessagePacker.UnpackId(networkReader);
                    // logging
                    if (logNetworkMessages) Debug.Log("ConnectionRecv " + this + " msgType:" + msgType + " content:" + BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count));

                    // try to invoke the handler for that message
                    if (InvokeHandler(msgType, networkReader, channelId))
                    {
                        lastMessageTime = Time.time;
                    }
                }
                catch (IOException ex)
                {
                    Debug.LogError("Closed connection: " + this + ". Invalid message " + ex);
                    Disconnect();
                }
            }
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
