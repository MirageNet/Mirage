using System;
using System.Collections.Generic;
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
    public class NetworkServer : MonoBehaviour
    {
        bool initialized;

        public class NetworkConnectionEvent : UnityEvent<NetworkConnectionToClient> { }


        /// <summary>
        /// The maximum number of concurrent network connections to support.
        /// <para>This effects the memory usage of the network layer.</para>
        /// </summary>
        [Tooltip("Maximum number of concurrent connections.")]
        [Min(1)]
        public int MaxConnections = 4;

        public NetworkConnectionEvent Connected = new NetworkConnectionEvent();


        [Header("Authentication")]
        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator authenticator;

        /// <summary>
        /// The connection to the host mode client (if any).
        /// </summary>
        // original HLAPI has .localConnections list with only m_LocalConnection in it
        // (for backwards compatibility because they removed the real localConnections list a while ago)
        // => removed it for easier code. use .localConnection now!
        public NetworkConnectionToClient localConnection { get; private set; }

        // The host client for this server 
        public NetworkClient localClient;

        /// <summary>
        /// True if there is a local client connected to this server (host mode)
        /// </summary>
        public bool LocalClientActive => localClient != null && localClient.active;

        /// <summary>
        /// A list of local connections on the server.
        /// </summary>
        public readonly Dictionary<int, NetworkConnectionToClient> connections = new Dictionary<int, NetworkConnectionToClient>();

        /// <summary>
        /// <para>Dictionary of the message handlers registered with the server.</para>
        /// <para>The key to the dictionary is the message Id.</para>
        /// </summary>
        readonly Dictionary<int, NetworkMessageDelegate> handlers = new Dictionary<int, NetworkMessageDelegate>();

        /// <summary>
        /// <para>If you enable this, the server will not listen for incoming connections on the regular network port.</para>
        /// <para>This can be used if the game is running in host mode and does not want external players to be able to connect - making it like a single-player game. Also this can be useful when using AddExternalConnection().</para>
        /// </summary>
        public bool dontListen;

        /// <summary>
        /// <para>Checks if the server has been started.</para>
        /// <para>This will be true after NetworkServer.Listen() has been called.</para>
        /// </summary>
        public bool active { get; private set; }

        public readonly Dictionary<uint, NetworkIdentity> spawned = new Dictionary<uint, NetworkIdentity>();

        // just a cached memory area where we can collect connections
        // for broadcasting messages
        private static readonly List<NetworkConnection> connectionsCache = new List<NetworkConnection>();

        // Time kept in this server
        public readonly NetworkTime Time = new NetworkTime();

        /// <summary>
        /// This shuts down the server and disconnects all clients.
        /// </summary>
        public void Shutdown()
        {
            if (initialized)
            {
                DisconnectAll();

                if (dontListen)
                {
                    // was never started, so dont stop
                }
                else
                {
                    // stop the server.
                    // we do NOT call Transport.Shutdown, because someone only
                    // called NetworkServer.Shutdown. we can't assume that the
                    // client is supposed to be shut down too!
                    Transport.activeTransport.ServerStop();
                }

                Transport.activeTransport.OnServerDisconnected.RemoveListener(OnDisconnected);
                Transport.activeTransport.OnServerConnected.RemoveListener(OnConnected);
                Transport.activeTransport.OnServerDataReceived.RemoveListener(OnDataReceived);
                Transport.activeTransport.OnServerError.RemoveListener(OnError);

                initialized = false;
            }

            dontListen = false;
            active = false;

            NetworkIdentity.ResetNextNetworkId();
        }

        void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            if (LogFilter.Debug) Debug.Log("NetworkServer Created version " + Version.Current);

            //Make sure connections are cleared in case any old connections references exist from previous sessions
            connections.Clear();
            Transport.activeTransport.OnServerDisconnected.AddListener(OnDisconnected);
            Transport.activeTransport.OnServerConnected.AddListener(OnConnected);
            Transport.activeTransport.OnServerDataReceived.AddListener(OnDataReceived);
            Transport.activeTransport.OnServerError.AddListener(OnError);
        }


        internal void RegisterMessageHandlers()
        {
            RegisterHandler<ReadyMessage>(OnClientReadyMessage);
            RegisterHandler<CommandMessage>(OnCommandMessage);
            RegisterHandler<RemovePlayerMessage>(OnRemovePlayerMessage);
            RegisterHandler<NetworkPingMessage>(Time.OnServerPing, false);
        }

        /// <summary>
        /// Start the server, setting the maximum number of connections.
        /// </summary>
        /// <param name="maxConns">Maximum number of allowed connections</param>
        /// <returns></returns>
        public void Listen()
        {
            Initialize();

            // only start server if we want to listen
            if (!dontListen)
            {
                Transport.activeTransport.ServerStart();
                if (LogFilter.Debug) Debug.Log("Server started listening");
            }

            active = true;
            RegisterMessageHandlers();
        }

        /// <summary>
        /// <para>This accepts a network connection and adds it to the server.</para>
        /// <para>This connection will use the callbacks registered with the server.</para>
        /// </summary>
        /// <param name="conn">Network connection to add.</param>
        /// <returns>True if added.</returns>
        public bool AddConnection(NetworkConnectionToClient conn)
        {
            if (!connections.ContainsKey(conn.connectionId))
            {
                // connection cannot be null here or conn.connectionId
                // would throw NRE
                connections[conn.connectionId] = conn;
                conn.SetHandlers(handlers);
                return true;
            }
            // already a connection with this id
            return false;
        }

        /// <summary>
        /// This removes an external connection added with AddExternalConnection().
        /// </summary>
        /// <param name="connectionId">The id of the connection to remove.</param>
        /// <returns>True if the removal succeeded</returns>
        public bool RemoveConnection(int connectionId)
        {
            return connections.Remove(connectionId);
        }

        // called by LocalClient to add itself. dont call directly.
        internal void SetLocalConnection(NetworkClient client, ULocalConnectionToClient conn)
        {
            if (localConnection != null)
            {
                Debug.LogError("Local Connection already exists");
                return;
            }

            localConnection = conn;
            localClient = client;
        }

        internal void RemoveLocalConnection()
        {
            if (localConnection != null)
            {
                localConnection.Disconnect();
                localConnection.Dispose();
                localConnection = null;
            }
            RemoveConnection(0);
            this.localClient = null;
        }

        internal void ActivateHostScene()
        {
            foreach (NetworkIdentity identity in spawned.Values)
            {
                if (!identity.isClient)
                {
                    if (LogFilter.Debug) Debug.Log("ActivateHostScene " + identity.netId + " " + identity);

                    identity.OnStartClient();
                }
            }
        }

        // this is like SendToReady - but it doesn't check the ready flag on the connection.
        // this is used for ObjectDestroy messages.
        void SendToObservers<T>(NetworkIdentity identity, T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            if (LogFilter.Debug) Debug.Log("Server.SendToObservers id:" + typeof(T));

            if (identity.observers != null)
                NetworkConnection.Send(identity.observers, msg, channelId);
        }

        /// <summary>
        /// Send a message structure with the given type number to all connected clients.
        /// <para>This applies to clients that are ready and not-ready.</para>
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="msg">Message structure.</param>
        /// <param name="channelId">Transport channel to use</param>
        /// <returns></returns>
        public bool SendToAll<T>(T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            if (LogFilter.Debug) Debug.Log("Server.SendToAll id:" + typeof(T));
            return NetworkConnection.Send(connections.Values, msg, channelId);
        }

        /// <summary>
        /// Send a message structure with the given type number to only clients which are ready.
        /// <para>See Networking.NetworkClient.Ready.</para>
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="identity"></param>
        /// <param name="msg">Message structure.</param>
        /// <param name="includeOwner">Send to observers including self..</param>
        /// <param name="channelId">Transport channel to use</param>
        /// <returns></returns>
        public bool SendToReady<T>(NetworkIdentity identity, T msg, bool includeOwner = true, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            if (LogFilter.Debug) Debug.Log("Server.SendToReady msgType:" + typeof(T));

            connectionsCache.Clear();
           
            foreach (NetworkConnection connection in identity.observers)
            {
                
                bool isOwner = connection == identity.connectionToClient;
                if ((!isOwner || includeOwner) && connection.isReady)
                {
                    connectionsCache.Add(connection);
                }
            }

            return NetworkConnection.Send(connectionsCache, msg, channelId);
        }

        /// <summary>
        /// Send a message structure with the given type number to only clients which are ready.
        /// <para>See Networking.NetworkClient.Ready.</para>
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="identity"></param>
        /// <param name="msg">Message structure.</param>
        /// <param name="channelId">Transport channel to use</param>
        /// <returns></returns>
        public bool SendToReady<T>(NetworkIdentity identity, T msg, int channelId) where T : IMessageBase
        {
            return SendToReady(identity, msg, true, channelId);
        }

        /// <summary>
        /// Disconnect all currently connected clients, including the local connection.
        /// <para>This can only be called on the server. Clients will receive the Disconnect message.</para>
        /// </summary>
        public void DisconnectAll()
        {
            DisconnectAllConnections();
            localConnection = null;
            localClient = null;
            active = false;

        }

        /// <summary>
        /// Disconnect all currently connected clients except the local connection.
        /// <para>This can only be called on the server. Clients will receive the Disconnect message.</para>
        /// </summary>
        public void DisconnectAllConnections()
        {
            foreach (NetworkConnection conn in connections.Values)
            {
                conn.Disconnect();
                // call OnDisconnected unless local player in host mode
                if (conn.connectionId != 0)
                    OnDisconnected(conn);
                conn.Dispose();
            }
            connections.Clear();
        }

        // The user should never need to pump the update loop manually
        internal void Update()
        {
            if (!active)
                return;

            // update all server objects
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in spawned)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    kvp.Value.ServerUpdate();
                }
                else
                {
                    // spawned list should have no null entries because we
                    // always call Remove in OnObjectDestroy everywhere.
                    Debug.LogWarning("Found 'null' entry in spawned list for netId=" + kvp.Key + ". Please call NetworkServer.Destroy to destroy networked objects. Don't use GameObject.Destroy.");
                }
            }
        }

        void OnConnected(int connectionId)
        {
            if (LogFilter.Debug) Debug.Log("Server accepted client:" + connectionId);

            // connectionId needs to be > 0 because 0 is reserved for local player
            if (connectionId <= 0)
            {
                Debug.LogError("Server.HandleConnect: invalid connectionId: " + connectionId + " . Needs to be >0, because 0 is reserved for local player.");
                Transport.activeTransport.ServerDisconnect(connectionId);
                return;
            }

            // connectionId not in use yet?
            if (connections.ContainsKey(connectionId))
            {
                Transport.activeTransport.ServerDisconnect(connectionId);
                if (LogFilter.Debug) Debug.Log("Server connectionId " + connectionId + " already in use. kicked client:" + connectionId);
                return;
            }

            // are more connections allowed? if not, kick
            // (it's easier to handle this in Mirror, so Transports can have
            //  less code and third party transport might not do that anyway)
            // (this way we could also send a custom 'tooFull' message later,
            //  Transport can't do that)
            if (connections.Count < MaxConnections)
            {
                // add connection
                var conn = new NetworkConnectionToClient(connectionId);
                OnConnected(conn);
            }
            else
            {
                // kick
                Transport.activeTransport.ServerDisconnect(connectionId);
                if (LogFilter.Debug) Debug.Log("Server full, kicked client:" + connectionId);
            }
        }

        internal void OnConnected(NetworkConnectionToClient conn)
        {
            if (LogFilter.Debug) Debug.Log("Server accepted client:" + conn);

            // add connection and invoke connected event
            AddConnection(conn);
            Connected.Invoke(conn);
        }

        void OnDisconnected(int connectionId)
        {
            if (LogFilter.Debug) Debug.Log("Server disconnect client:" + connectionId);

            if (connections.TryGetValue(connectionId, out NetworkConnectionToClient conn))
            {
                conn.Disconnect();
                RemoveConnection(connectionId);
                if (LogFilter.Debug) Debug.Log("Server lost client:" + connectionId);

                OnDisconnected(conn);
            }
        }

        void OnDisconnected(NetworkConnection conn)
        {
            conn.InvokeHandler(new DisconnectMessage(), -1);
            if (LogFilter.Debug) Debug.Log("Server lost client:" + conn);
        }

        /// <summary>
        /// server that received the message
        /// </summary>
        /// <remarks>This is a hack, but it is needed to deserialize
        /// gameobjects when processing the message</remarks>
        /// 
        internal static NetworkServer Current;

        void OnDataReceived(int connectionId, ArraySegment<byte> data, int channelId)
        {
            if (connections.TryGetValue(connectionId, out NetworkConnectionToClient conn))
            {
                conn.TransportReceive(data, channelId);
            }
            else
            {
                Debug.LogError("HandleData Unknown connectionId:" + connectionId);
            }
        }

        void OnError(int connectionId, Exception exception)
        {
            // TODO Let's discuss how we will handle errors
            Debug.LogException(exception);
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<NetworkConnectionToClient, T> handler, bool requireAuthentication = true) where T : IMessageBase, new()
        {
            int msgType = MessagePacker.GetId<T>();
            if (handlers.ContainsKey(msgType))
            {
                if (LogFilter.Debug) Debug.Log("NetworkServer.RegisterHandler replacing " + msgType);
            }
            handlers[msgType] = MessagePacker.MessageHandler(handler, requireAuthentication);
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
            RegisterHandler<T>((_, value) => { handler(value); }, requireAuthentication);
        }

        /// <summary>
        /// Unregisters a handler for a particular message type.
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        public void UnregisterHandler<T>() where T : IMessageBase
        {
            int msgType = MessagePacker.GetId<T>();
            handlers.Remove(msgType);
        }

        /// <summary>
        /// Clear all registered callback handlers.
        /// </summary>
        public void ClearHandlers()
        {
            handlers.Clear();
        }

        /// <summary>
        /// send this message to the player only
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="identity"></param>
        /// <param name="msg"></param>
        public void SendToClientOfPlayer<T>(NetworkIdentity identity, T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            if (identity != null)
            {
                identity.connectionToClient.Send(msg, channelId);
            }
            else
            {
                Debug.LogError("SendToClientOfPlayer: player has no NetworkIdentity: " + identity);
            }
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="conn">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param> 
        /// <param name="player">Player object spawned for the player.</param>
        /// <param name="assetId"></param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public bool ReplacePlayerForConnection(NetworkConnection conn, NetworkClient client, GameObject player, Guid assetId, bool keepAuthority = false)
        {
            if (GetNetworkIdentity(player, out NetworkIdentity identity))
            {
                identity.assetId = assetId;
            }
            return InternalReplacePlayerForConnection(conn, client, player, keepAuthority);
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="conn">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param> 
        /// <param name="player">Player object spawned for the player.</param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public bool ReplacePlayerForConnection(NetworkConnection conn, NetworkClient client, GameObject player, bool keepAuthority = false)
        {
            return InternalReplacePlayerForConnection(conn, client, player, keepAuthority);
        }

        /// <summary>
        /// <para>When an AddPlayer message handler has received a request from a player, the server calls this to associate the player object with the connection.</para>
        /// <para>When a player is added for a connection, the client for that connection is made ready automatically. The player object is automatically spawned, so you do not need to call NetworkServer.Spawn for that object. This function is used for "adding" a player, not for "replacing" the player on a connection. If there is already a player on this playerControllerId for this connection, this will fail.</para>
        /// </summary>
        /// <param name="conn">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param> 
        /// <param name="player">Player object spawned for the player.</param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public bool AddPlayerForConnection(NetworkConnection conn, GameObject player, Guid assetId)
        {
            if (GetNetworkIdentity(player, out NetworkIdentity identity))
            {
                identity.assetId = assetId;
            }
            return AddPlayerForConnection(conn, player);
        }

        void SpawnObserversForConnection(NetworkConnection conn)
        {
            if (LogFilter.Debug) Debug.Log("Spawning " + spawned.Count + " objects for conn " + conn);

            if (!conn.isReady)
            {
                // client needs to finish initializing before we can spawn objects
                // otherwise it would not find them.
                return;
            }

            // let connection know that we are about to start spawning...
            conn.Send(new ObjectSpawnStartedMessage());

            // add connection to each nearby NetworkIdentity's observers, which
            // internally sends a spawn message for each one to the connection.
            foreach (NetworkIdentity identity in spawned.Values)
            {
                // try with far away ones in ummorpg!
                //TODO this is different
                if (identity.gameObject.activeSelf)
                {
                    if (LogFilter.Debug) Debug.Log("Sending spawn message for current server objects name='" + identity.name + "' netId=" + identity.netId + " sceneId=" + identity.sceneId);

                    bool visible = identity.OnCheckObserver(conn);
                    if (visible)
                    {
                        identity.AddObserver(conn);
                    }
                }
            }

            // let connection know that we finished spawning, so it can call
            // OnStartClient on each one (only after all were spawned, which
            // is how Unity's Start() function works too)
            conn.Send(new ObjectSpawnFinishedMessage());
        }

        /// <summary>
        /// <para>When an AddPlayer message handler has received a request from a player, the server calls this to associate the player object with the connection.</para>
        /// <para>When a player is added for a connection, the client for that connection is made ready automatically. The player object is automatically spawned, so you do not need to call NetworkServer.Spawn for that object. This function is used for "adding" a player, not for "replacing" the player on a connection. If there is already a player on this playerControllerId for this connection, this will fail.</para>
        /// </summary>
        /// <param name="conn">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param>
        /// <param name="player">Player object spawned for the player.</param>
        /// <returns></returns>
        public bool AddPlayerForConnection(NetworkConnection conn, GameObject player)
        {
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.Log("AddPlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + player);
                return false;
            }
            identity.Reset();

            // cannot have a player object in "Add" version
            if (conn.identity != null)
            {
                Debug.Log("AddPlayer: player object already exists");
                return false;
            }

            // make sure we have a controller before we call SetClientReady
            // because the observers will be rebuilt only if we have a controller
            conn.identity = identity;

            // set server to the NetworkIdentity
            identity.server = this;

            identity.client = this.localClient;

            // Set the connection on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients)
            identity.SetClientOwner(conn);

            // special case,  we are in host mode,  set hasAuthority to true so that all overrides see it
            if (conn is ULocalConnectionToClient)
            {
                identity.hasAuthority = true;
                this.localClient.InternalAddPlayer(identity);
            }

            // set ready if not set yet
            SetClientReady(conn);

            if (LogFilter.Debug) Debug.Log("Adding new playerGameObject object netId: " + identity.netId + " asset ID " + identity.assetId);

            Respawn(identity);
            return true;
        }

        void Respawn(NetworkIdentity identity)
        {
            if (identity.netId == 0)
            {
                // If the object has not been spawned, then do a full spawn and update observers
                Spawn(identity.gameObject, identity.connectionToClient);
            }
            else
            {
                // otherwise just replace his data
                SendSpawnMessage(identity, identity.connectionToClient);
            }
        }

        internal bool InternalReplacePlayerForConnection(NetworkConnection conn, NetworkClient client, GameObject player, bool keepAuthority)
        {
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("ReplacePlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + player);
                return false;
            }

            if (identity.connectionToClient != null && identity.connectionToClient != conn)
            {
                Debug.LogError("Cannot replace player for connection. New player is already owned by a different connection" + player);
                return false;
            }

            //NOTE: there can be an existing player
            if (LogFilter.Debug) Debug.Log("NetworkServer ReplacePlayer");

            NetworkIdentity previousPlayer = conn.identity;

            conn.identity = identity;
            identity.client = client;

            // Set the connection on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients)
            identity.SetClientOwner(conn);

            //NOTE: DONT set connection ready.

            // special case,  we are in host mode,  set hasAuthority to true so that all overrides see it
            if (conn is ULocalConnectionToClient)
            {
                identity.hasAuthority = true;
                client.InternalAddPlayer(identity);
            }

            // add connection to observers AFTER the playerController was set.
            // by definition, there is nothing to observe if there is no player
            // controller.
            //
            // IMPORTANT: do this in AddPlayerForConnection & ReplacePlayerForConnection!
            SpawnObserversForConnection(conn);

            if (LogFilter.Debug) Debug.Log("Replacing playerGameObject object netId: " + player.GetComponent<NetworkIdentity>().netId + " asset ID " + player.GetComponent<NetworkIdentity>().assetId);

            Respawn(identity);

            if (!keepAuthority)
                previousPlayer.RemoveClientAuthority();

            return true;
        }

        internal bool GetNetworkIdentity(GameObject go, out NetworkIdentity identity)
        {
            identity = go.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("GameObject " + go.name + " doesn't have NetworkIdentity.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sets the client to be ready.
        /// <para>When a client has signaled that it is ready, this method tells the server that the client is ready to receive spawned objects and state synchronization updates. This is usually called in a handler for the SYSTEM_READY message. If there is not specific action a game needs to take for this message, relying on the default ready handler function is probably fine, so this call wont be needed.</para>
        /// </summary>
        /// <param name="conn">The connection of the client to make ready.</param>
        public void SetClientReady(NetworkConnection conn)
        {
            if (LogFilter.Debug) Debug.Log("SetClientReadyInternal for conn:" + conn);

            // set ready
            conn.isReady = true;

            // client is ready to start spawning objects
            if (conn.identity != null)
                SpawnObserversForConnection(conn);
        }

        internal void ShowForConnection(NetworkIdentity identity, NetworkConnection conn)
        {
            if (conn.isReady)
                SendSpawnMessage(identity, conn);
        }

        internal void HideForConnection(NetworkIdentity identity, NetworkConnection conn)
        {
            var msg = new ObjectHideMessage
            {
                netId = identity.netId
            };
            conn.Send(msg);
        }

        /// <summary>
        /// Marks all connected clients as no longer ready.
        /// <para>All clients will no longer be sent state synchronization updates. The player's clients can call ClientManager.Ready() again to re-enter the ready state. This is useful when switching scenes.</para>
        /// </summary>
        public void SetAllClientsNotReady()
        {
            foreach (NetworkConnection conn in connections.Values)
            {
                SetClientNotReady(conn);
            }
        }

        /// <summary>
        /// Sets the client of the connection to be not-ready.
        /// <para>Clients that are not ready do not receive spawned objects or state synchronization updates. They client can be made ready again by calling SetClientReady().</para>
        /// </summary>
        /// <param name="conn">The connection of the client to make not ready.</param>
        public void SetClientNotReady(NetworkConnection conn)
        {
            if (conn.isReady)
            {
                if (LogFilter.Debug) Debug.Log("PlayerNotReady " + conn);
                conn.isReady = false;
                conn.RemoveObservers();

                conn.Send(new NotReadyMessage());
            }
        }

        // default ready handler.
        void OnClientReadyMessage(NetworkConnection conn, ReadyMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("Default handler for ready message from " + conn);
            SetClientReady(conn);
        }

        // default remove player handler
        void OnRemovePlayerMessage(NetworkConnection conn, RemovePlayerMessage msg)
        {
            if (conn.identity != null)
            {
                Destroy(conn.identity.gameObject);
                conn.identity = null;
            }
            else
            {
                Debug.LogError("Received remove player message but connection has no player");
            }
        }

        // Handle command from specific player, this could be one of multiple players on a single client
        void OnCommandMessage(NetworkConnection conn, CommandMessage msg)
        {
            if (!spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                Debug.LogWarning("Spawned object not found when handling Command message [netId=" + msg.netId + "]");
                return;
            }

            // Commands can be for player objects, OR other objects with client-authority
            // -> so if this connection's controller has a different netId then
            //    only allow the command if clientAuthorityOwner
            if (identity.connectionToClient != conn)
            {
                Debug.LogWarning("Command for object without authority [netId=" + msg.netId + "]");
                return;
            }

            if (LogFilter.Debug) Debug.Log("OnCommandMessage for netId=" + msg.netId + " conn=" + conn);

            using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                identity.HandleCommand(msg.componentIndex, msg.functionHash, networkReader);
        }

        internal void SpawnObject(GameObject obj, NetworkConnection ownerConnection)
        {
            if (!active)
            {
                Debug.LogError("SpawnObject for " + obj + ", NetworkServer is not active. Cannot spawn objects without an active server.");
                return;
            }

            NetworkIdentity identity = obj.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("SpawnObject " + obj + " has no NetworkIdentity. Please add a NetworkIdentity to " + obj);
                return;
            }
            identity.Reset();
            identity.connectionToClient = (NetworkConnectionToClient)ownerConnection;
            identity.server = this;
            identity.client = localClient;

            // special case to make sure hasAuthority is set
            // on start server in host mode
            if (ownerConnection is ULocalConnectionToClient)
                identity.hasAuthority = true;

            identity.OnStartServer();

            if (LogFilter.Debug) Debug.Log("SpawnObject instance ID " + identity.netId + " asset ID " + identity.assetId);

            identity.RebuildObservers(true);
        }

        internal void SendSpawnMessage(NetworkIdentity identity, NetworkConnection conn)
        {
            if (identity.serverOnly)
                return;

            // for easier debugging
            if (LogFilter.Debug) Debug.Log("Server SendSpawnMessage: name=" + identity.name + " sceneId=" + identity.sceneId.ToString("X") + " netid=" + identity.netId);

            // one writer for owner, one for observers
            using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter(), observersWriter = NetworkWriterPool.GetWriter())
            {
                // serialize all components with initialState = true
                // (can be null if has none)
                (int ownerWritten, int observersWritten) = identity.OnSerializeAllSafely(true, ownerWriter, observersWriter);

                // convert to ArraySegment to avoid reader allocations
                // (need to handle null case too)
                ArraySegment<byte> ownerSegment = ownerWritten > 0 ? ownerWriter.ToArraySegment() : default;
                ArraySegment<byte> observersSegment = observersWritten > 0 ? observersWriter.ToArraySegment() : default;

                var msg = new SpawnMessage
                {
                    netId = identity.netId,
                    isLocalPlayer = conn.identity == identity,
                    isOwner = identity.connectionToClient == conn,
                    sceneId = identity.sceneId,
                    assetId = identity.assetId,
                    // use local values for VR support
                    position = identity.transform.localPosition,
                    rotation = identity.transform.localRotation,
                    scale = identity.transform.localScale
                };

                // use owner segment if 'conn' owns this identity, otherwise
                // use observers segment
                msg.payload = msg.isOwner ? ownerSegment : observersSegment;

                conn.Send(msg);
            }
        }

        /// <summary>
        /// This destroys all the player objects associated with a NetworkConnections on a server.
        /// <para>This is used when a client disconnects, to remove the players for that client. This also destroys non-player objects that have client authority set for this connection.</para>
        /// </summary>
        /// <param name="conn">The connections object to clean up for.</param>
        public void DestroyPlayerForConnection(NetworkConnection conn)
        {
            // destroy all objects owned by this connection
            conn.DestroyOwnedObjects();

            if (conn.identity != null)
            {
                DestroyObject(conn.identity, true);
                conn.identity = null;
            }
        }

        bool CheckForPrefab(GameObject obj)
        {
#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(obj);
#elif UNITY_2018_2_OR_NEWER
            return (UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj) == null) && (UnityEditor.PrefabUtility.GetPrefabObject(obj) != null);
#else
            return (UnityEditor.PrefabUtility.GetPrefabParent(obj) == null) && (UnityEditor.PrefabUtility.GetPrefabObject(obj) != null);
#endif
#else
            return false;
#endif
        }

        bool VerifyCanSpawn(GameObject obj)
        {
            if (CheckForPrefab(obj))
            {
                Debug.LogErrorFormat("GameObject {0} is a prefab, it can't be spawned. This will cause errors in builds.", obj.name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This spawns an object like NetworkServer.Spawn() but also assigns Client Authority to the specified client.
        /// <para>This is the same as calling NetworkIdentity.AssignClientAuthority on the spawned object.</para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="player">The player object to set Client Authority to.</param>
        public void Spawn(GameObject obj, GameObject player)
        {
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("Player object has no NetworkIdentity");
                return;
            }

            if (identity.connectionToClient == null)
            {
                Debug.LogError("Player object is not a player.");
                return;
            }

            Spawn(obj, identity.connectionToClient);
        }

        /// <summary>
        /// This spawns an object like NetworkServer.Spawn() but also assigns Client Authority to the specified client.
        /// <para>This is the same as calling NetworkIdentity.AssignClientAuthority on the spawned object.</para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="assetId">The assetId of the object to spawn. Used for custom spawn handlers.</param>
        /// <param name="client">The client associated to the object.</param>
        /// <param name="ownerConnection">The connection that has authority over the object</param>
        public void Spawn(GameObject obj, Guid assetId, NetworkConnection ownerConnection = null)
        {
            if (VerifyCanSpawn(obj))
            {
                if (GetNetworkIdentity(obj, out NetworkIdentity identity))
                {
                    identity.assetId = assetId;
                }
                SpawnObject(obj, ownerConnection);
            }
        }

        /// <summary>
        /// Spawn the given game object on all clients which are ready.
        /// <para>This will cause a new object to be instantiated from the registered prefab, or from a custom spawn function.</para>
        /// </summary>
        /// <param name="obj">Game object with NetworkIdentity to spawn.</param>
        /// <param name="client">Client associated to the object.</param>
        /// <param name="ownerConnection">The connection that has authority over the object</param>
        public void Spawn(GameObject obj, NetworkConnection ownerConnection = null)
        {
            if (VerifyCanSpawn(obj))
            {
                SpawnObject(obj, ownerConnection);
            }
        }

        void DestroyObject(NetworkIdentity identity, bool destroyServerObject)
        {
            if (LogFilter.Debug) Debug.Log("DestroyObject instance:" + identity.netId);
            spawned.Remove(identity.netId);

            identity.connectionToClient?.RemoveOwnedObject(identity);

            var msg = new ObjectDestroyMessage
            {
                netId = identity.netId
            };
            SendToObservers(identity, msg);

            identity.ClearObservers();
            if (LocalClientActive)
            {
                identity.OnNetworkDestroy();
            }

            // when unspawning, dont destroy the server's object
            if (destroyServerObject)
            {
                UnityEngine.Object.Destroy(identity.gameObject);
            }
            identity.MarkForReset();
        }

        /// <summary>
        /// Destroys this object and corresponding objects on all clients.
        /// <para>In some cases it is useful to remove an object but not delete it on the server. For that, use NetworkServer.UnSpawn() instead of NetworkServer.Destroy().</para>
        /// </summary>
        /// <param name="obj">Game object to destroy.</param>
        public void Destroy(GameObject obj)
        {
            if (obj == null)
            {
                if (LogFilter.Debug) Debug.Log("NetworkServer DestroyObject is null");
                return;
            }

            if (GetNetworkIdentity(obj, out NetworkIdentity identity))
            {
                DestroyObject(identity, true);
            }
        }

        /// <summary>
        /// This takes an object that has been spawned and un-spawns it.
        /// <para>The object will be removed from clients that it was spawned on, or the custom spawn handler function on the client will be called for the object.</para>
        /// <para>Unlike when calling NetworkServer.Destroy(), on the server the object will NOT be destroyed. This allows the server to re-use the object, even spawn it again later.</para>
        /// </summary>
        /// <param name="obj">The spawned object to be unspawned.</param>
        public void UnSpawn(GameObject obj)
        {
            if (obj == null)
            {
                if (LogFilter.Debug) Debug.Log("NetworkServer UnspawnObject is null");
                return;
            }

            if (GetNetworkIdentity(obj, out NetworkIdentity identity))
            {
                DestroyObject(identity, false);
            }
        }

        internal bool ValidateSceneObject(NetworkIdentity identity)
        {
            if (identity.gameObject.hideFlags == HideFlags.NotEditable ||
                identity.gameObject.hideFlags == HideFlags.HideAndDontSave)
                return false;

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(identity.gameObject))
                return false;
#endif

            // If not a scene object
            return identity.sceneId != 0;
        }

        /// <summary>
        /// This causes NetworkIdentity objects in a scene to be spawned on a server.
        /// <para>NetworkIdentity objects in a scene are disabled by default. Calling SpawnObjects() causes these scene objects to be enabled and spawned. It is like calling NetworkServer.Spawn() for each of them.</para>
        /// </summary>
        /// <param name="client">The client associated to the objects.</param>
        /// <returns>Success if objects where spawned.</returns>
        public bool SpawnObjects()
        {
            // only if server active
            if (!active)
                return false;

            NetworkIdentity[] identities = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
            foreach (NetworkIdentity identity in identities)
            {
                if (ValidateSceneObject(identity))
                {
                    if (LogFilter.Debug) Debug.Log("SpawnObjects sceneId:" + identity.sceneId.ToString("X") + " name:" + identity.gameObject.name);
                    identity.Reset();
                    identity.gameObject.SetActive(true);
                }
            }

            foreach (NetworkIdentity identity in identities)
            {
                if (ValidateSceneObject(identity))
                    Spawn(identity.gameObject);
            }
            return true;
        }
    }
}
