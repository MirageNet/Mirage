using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

namespace Mirror
{

    /// <summary>
    /// The NetworkServer.
    /// </summary>
    /// <remarks>
    /// <para>NetworkServer handles remote connections from remote clients via a NetworkServerSimple instance, and also has a local connection for a local client.</para>
    /// <para>The NetworkManager uses the NetworkServer, but it can be used without the NetworkManager.</para>
    /// <para>The set of networked objects that have been spawned is managed by NetworkServer. Objects are spawned with NetworkServer.Spawn() which adds them to this set, and makes them be created on clients. Spawned objects are removed automatically when they are destroyed, or than they can be removed from the spawned set by calling NetworkServer.UnSpawn() - this does not destroy the object.</para>
    /// <para>There are a number of internal messages used by NetworkServer, these are setup when NetworkServer.Listen() is called.</para>
    /// </remarks>
    [DisallowMultipleComponent]
    public class NetworkServer : MonoBehaviour, INetworkServer
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkServer));

        bool initialized;

        /// <summary>
        /// The maximum number of concurrent network connections to support.
        /// <para>This effects the memory usage of the network layer.</para>
        /// </summary>
        [Tooltip("Maximum number of concurrent connections.")]
        [Min(1)]
        public int MaxConnections = 4;

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// </summary>
        public UnityEvent Started = new UnityEvent();

        /// <summary>
        /// Event fires once a new Client has connect to the Server.
        /// </summary>
        public NetworkConnectionEvent Connected = new NetworkConnectionEvent();

        /// <summary>
        /// Event fires once a new Client has passed Authentication to the Server.
        /// </summary>
        public NetworkConnectionEvent Authenticated = new NetworkConnectionEvent();

        /// <summary>
        /// Event fires once a Client has Disconnected from the Server.
        /// </summary>
        public NetworkConnectionEvent Disconnected = new NetworkConnectionEvent();

        public UnityEvent Stopped = new UnityEvent();

        [Header("Authentication")]
        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator authenticator;

        /// <summary>
        /// The connection to the host mode client (if any).
        /// </summary>
        // original HLAPI has .localConnections list with only m_LocalConnection in it
        // (for backwards compatibility because they removed the real localConnections list a while ago)
        // => removed it for easier code. use .localConnection now!
        public INetworkConnection LocalConnection { get; private set; }

        // The host client for this server 
        public NetworkClient LocalClient { get; private set; }

        /// <summary>
        /// True if there is a local client connected to this server (host mode)
        /// </summary>
        public bool LocalClientActive => LocalClient != null && LocalClient.Active;

        /// <summary>
        /// Number of active player objects across all connections on the server.
        /// <para>This is only valid on the host / server.</para>
        /// </summary>
        public int NumPlayers => connections.Count(kv => kv.Identity != null);

        /// <summary>
        /// A list of local connections on the server.
        /// </summary>
        public readonly HashSet<INetworkConnection> connections = new HashSet<INetworkConnection>();

        /// <summary>
        /// <para>If you disable this, the server will not listen for incoming connections on the regular network port.</para>
        /// <para>This can be used if the game is running in host mode and does not want external players to be able to connect - making it like a single-player game. Also this can be useful when using AddExternalConnection().</para>
        /// </summary>
        public bool Listening = true;

        /// <summary>
        /// <para>Checks if the server has been started.</para>
        /// <para>This will be true after NetworkServer.Listen() has been called.</para>
        /// </summary>
        public bool Active { get; private set; }

        public readonly Dictionary<uint, NetworkIdentity> Spawned = new Dictionary<uint, NetworkIdentity>();

        // just a cached memory area where we can collect connections
        // for broadcasting messages
        private static readonly List<INetworkConnection> connectionsCache = new List<INetworkConnection>();

        // Time kept in this server
        public readonly NetworkTime Time = new NetworkTime();

        // transport to use to accept connections
        public Transport transport;

        /// <summary>
        /// This shuts down the server and disconnects all clients.
        /// </summary>
        public void Disconnect()
        {
            foreach (INetworkConnection conn in connections)
            {
                conn.Disconnect();
            }
            if (transport != null)
                transport.Disconnect();
        }
        
        void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            if (logger.LogEnabled()) logger.Log("NetworkServer Created version " + Version.Current);

            //Make sure connections are cleared in case any old connections references exist from previous sessions
            connections.Clear();

            if (transport == null)
                transport = GetComponent<Transport>();

            if (authenticator != null)
            {
                authenticator.OnServerAuthenticated += OnAuthenticated;

                Connected.AddListener(authenticator.OnServerAuthenticateInternal);
            }
            else
            {
                // if no authenticator, consider every connection as authenticated
                Connected.AddListener(OnAuthenticated);
            }
        }

        internal void RegisterMessageHandlers(INetworkConnection connection)
        {
            connection.RegisterHandler<ReadyMessage>(OnClientReadyMessage);
        }

        /// <summary>
        /// Start the server, setting the maximum number of connections.
        /// </summary>
        /// <param name="maxConns">Maximum number of allowed connections</param>
        /// <returns></returns>
        public async Task ListenAsync()
        {
            Initialize();

            // only start server if we want to listen
            if (Listening)
            {
                await transport.ListenAsync();
                logger.Log("Server started listening");
            }

            Active = true;

            // call OnStartServer AFTER Listen, so that NetworkServer.active is
            // true and we can call NetworkServer.Spawn in OnStartServer
            // overrides.
            // (useful for loading & spawning stuff from database etc.)
            //
            // note: there is no risk of someone connecting after Listen() and
            //       before OnStartServer() because this all runs in one thread
            //       and we don't start processing connects until Update.
            Started.Invoke();

            _ = AcceptAsync();
        }

        // accept connections from clients
        private async Task AcceptAsync()
        {
            try
            {
                IConnection connection;

                while ((connection = await transport.AcceptAsync()) != null)
                {
                    var networkConnectionToClient = GetNewConnection(connection);

                    _ = ConnectionAcceptedAsync(networkConnectionToClient);
                }
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                Cleanup();
            }
        }

        /// <summary>
        /// cleanup resources so that we can start again
        /// </summary>
        private void Cleanup()
        {

            if (authenticator != null)
            {
                authenticator.OnServerAuthenticated -= OnAuthenticated;
                Connected.RemoveListener(authenticator.OnServerAuthenticateInternal);
            }
            else
            {
                // if no authenticator, consider every connection as authenticated
                Connected.RemoveListener(OnAuthenticated);
            }

            Stopped.Invoke();
            initialized = false;
            Active = false;
        }

        /// <summary>
        /// Creates a new INetworkConnection based on the provided IConnection.
        /// </summary>
        public virtual INetworkConnection GetNewConnection(IConnection connection)
        {
            return new NetworkConnection(connection);
        }

        /// <summary>
        /// <para>This accepts a network connection and adds it to the server.</para>
        /// <para>This connection will use the callbacks registered with the server.</para>
        /// </summary>
        /// <param name="conn">Network connection to add.</param>
        public void AddConnection(INetworkConnection conn)
        {
            if (!connections.Contains(conn))
            {
                // connection cannot be null here or conn.connectionId
                // would throw NRE
                connections.Add(conn);
                conn.RegisterHandler<NetworkPingMessage>(Time.OnServerPing);
            }
        }

        /// <summary>
        /// This removes an external connection added with AddExternalConnection().
        /// </summary>
        /// <param name="connectionId">The id of the connection to remove.</param>
        public void RemoveConnection(INetworkConnection conn)
        {
            connections.Remove(conn);
        }

        /// <summary>
        /// called by LocalClient to add itself. dont call directly.
        /// </summary>
        /// <param name="client">The local client</param>
        /// <param name="tconn">The connection to the client</param>
        internal void SetLocalConnection(NetworkClient client, IConnection tconn)
        {
            if (LocalConnection != null)
            {
                throw new InvalidOperationException("Local Connection already exists");
            }

            var conn = GetNewConnection(tconn);
            LocalConnection = conn;
            LocalClient = client;

            _ = ConnectionAcceptedAsync(conn);

        }

        /// <summary>
        /// Send a message to all connected clients, both ready and not-ready.
        /// <para>See <see cref="NetworkConnection.IsReady"/></para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="msg">Message</param>
        /// <param name="channelId">Transport channel to use</param>
        /// <param name="sendToReadyOnly">Indicates if only ready clients should receive the message</param>
        public void SendToAll<T>(T msg, int channelId = Channels.DefaultReliable)
        {
            if (logger.LogEnabled()) logger.Log("Server.SendToAll id:" + typeof(T));
            NetworkConnection.Send(connections, msg, channelId);
        }

        /// <summary>
        /// Send a message to only clients which are ready with option to include the owner of the object identity.
        /// <para>See <see cref="NetworkConnection.IsReady"/></para>
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="identity">Identity of the owner</param>
        /// <param name="msg">Message</param>
        /// <param name="includeOwner">Should the owner of the object be included</param>
        /// <param name="channelId">Transport channel to use</param>
        public void SendToReady<T>(NetworkIdentity identity, T msg, bool includeOwner = true, int channelId = Channels.DefaultReliable)
        {
            if (logger.LogEnabled()) logger.Log("Server.SendToReady msgType:" + typeof(T));

            connectionsCache.Clear();

            foreach (INetworkConnection connection in identity.observers)
            {
                bool isOwner = connection == identity.ConnectionToClient;
                if ((!isOwner || includeOwner) && connection.IsReady)
                {
                    connectionsCache.Add(connection);
                }
            }

            NetworkConnection.Send(connectionsCache, msg, channelId);
        }

        /// <summary>
        /// Send a message to only clients which are ready including the owner of the object identity.
        /// <para>See <see cref="NetworkConnection.IsReady"/></para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="identity">identity of the object</param>
        /// <param name="msg">Message</param>
        /// <param name="channelId">Transport channel to use</param>
        public void SendToReady<T>(NetworkIdentity identity, T msg, int channelId)
        {
            SendToReady(identity, msg, true, channelId);
        }

        // The user should never need to pump the update loop manually
        internal void Update()
        {
            if (!Active)
                return;

            // update all server objects
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in Spawned)
            {
                NetworkIdentity identity = kvp.Value;
                if (identity != null)
                {
                    identity.ServerUpdate();
                }
                else
                {
                    // spawned list should have no null entries because we
                    // always call Remove in OnObjectDestroy everywhere.
                    logger.LogWarning("Found 'null' entry in spawned list for netId=" + kvp.Key + ". Please call NetworkServer.Destroy to destroy networked objects. Don't use GameObject.Destroy.");
                }
            }
        }

        async Task ConnectionAcceptedAsync(INetworkConnection conn)
        {
            if (logger.LogEnabled()) logger.Log("Server accepted client:" + conn);

            // are more connections allowed? if not, kick
            // (it's easier to handle this in Mirror, so Transports can have
            //  less code and third party transport might not do that anyway)
            // (this way we could also send a custom 'tooFull' message later,
            //  Transport can't do that)
            if (connections.Count >= MaxConnections)
            {
                conn.Disconnect();
                if (logger.LogEnabled()) logger.Log("Server full, kicked client:" + conn);
                return;
            }

            // add connection
            AddConnection(conn);

            // let everyone know we just accepted a connection
            Connected.Invoke(conn);

            // now process messages until the connection closes
            try
            {
                await conn.ProcessMessagesAsync();
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                OnDisconnected(conn);
            }
        }

        //called once a client disconnects from the server
        void OnDisconnected(INetworkConnection connection)
        {
            if (logger.LogEnabled()) logger.Log("Server disconnect client:" + connection);

            RemoveConnection(connection);

            Disconnected.Invoke(connection);

            connection.DestroyOwnedObjects();
            connection.Identity = null;

            if (connection == LocalConnection)
                LocalConnection = null;
        }

        internal void OnAuthenticated(INetworkConnection conn)
        {
            if (logger.LogEnabled()) logger.Log("Server authenticate client:" + conn);

            // connection has been authenticated,  now we can handle other messages
            RegisterMessageHandlers(conn);

            Authenticated?.Invoke(conn);
        }

        /// <summary>
        /// server that received the message
        /// </summary>
        /// <remarks>This is a hack, but it is needed to deserialize
        /// gameobjects when processing the message</remarks>
        /// 
        internal static NetworkServer Current;

        /// <summary>
        /// Sets the client to be ready.
        /// <para>When a client has signaled that it is ready, this method tells the server that the client is ready to receive spawned objects and state synchronization updates. This is usually called in a handler for the SYSTEM_READY message. If there is not specific action a game needs to take for this message, relying on the default ready handler function is probably fine, so this call wont be needed.</para>
        /// </summary>
        /// <param name="conn">The connection of the client to make ready.</param>
        public void SetClientReady(INetworkConnection conn)
        {
            if (logger.LogEnabled()) logger.Log("SetClientReadyInternal for conn:" + conn);

            // set ready
            conn.IsReady = true;

            // client is ready to start spawning objects
            if (conn.Identity != null)
                SpawnObserversForConnection(conn);
        }

        /// <summary>
        /// Marks all connected clients as no longer ready.
        /// <para>All clients will no longer be sent state synchronization updates. The player's clients can call ClientManager.Ready() again to re-enter the ready state. This is useful when switching scenes.</para>
        /// </summary>
        public void SetAllClientsNotReady()
        {
            foreach (INetworkConnection conn in connections)
            {
                SetClientNotReady(conn);
            }
        }

        /// <summary>
        /// Sets the client of the connection to be not-ready.
        /// <para>Clients that are not ready do not receive spawned objects or state synchronization updates. They client can be made ready again by calling SetClientReady().</para>
        /// </summary>
        /// <param name="conn">The connection of the client to make not ready.</param>
        public void SetClientNotReady(INetworkConnection conn)
        {
            if (conn.IsReady)
            {
                if (logger.LogEnabled()) logger.Log("PlayerNotReady " + conn);
                conn.IsReady = false;
                conn.RemoveObservers();

                conn.Send(new NotReadyMessage());
            }
        }

        /// <summary>
        /// default ready handler. 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        void OnClientReadyMessage(INetworkConnection conn, ReadyMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("Default handler for ready message from " + conn);
            SetClientReady(conn);
        }
    }
}
