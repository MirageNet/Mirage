using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Guid = System.Guid;
using Object = UnityEngine.Object;

namespace Mirror
{
    public enum ConnectState
    {
        None,
        Connecting,
        Connected,
        Disconnected
    }

    /// <summary>
    /// This is a network client class used by the networking system. It contains a NetworkConnection that is used to connect to a network server.
    /// <para>The <see cref="NetworkClient">NetworkClient</see> handle connection state, messages handlers, and connection configuration. There can be many <see cref="NetworkClient">NetworkClient</see> instances in a process at a time, but only one that is connected to a game server (<see cref="NetworkServer">NetworkServer</see>) that uses spawned objects.</para>
    /// <para><see cref="NetworkClient">NetworkClient</see> has an internal update function where it handles events from the transport layer. This includes asynchronous connect events, disconnect events and incoming data from a server.</para>
    /// <para>The <see cref="NetworkManager">NetworkManager</see> has a NetworkClient instance that it uses for games that it starts, but the NetworkClient may be used by itself.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class NetworkClient : MonoBehaviour
    {

        [Header("Authentication")]
        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator authenticator;

        // spawn handlers
        readonly Dictionary<Guid, SpawnHandlerDelegate> spawnHandlers = new Dictionary<Guid, SpawnHandlerDelegate>();
        readonly Dictionary<Guid, UnSpawnDelegate> unspawnHandlers = new Dictionary<Guid, UnSpawnDelegate>();

        [Serializable] public class NetworkConnectionEvent : UnityEvent<NetworkConnectionToServer> { }

        public NetworkConnectionEvent Connected = new NetworkConnectionEvent();
        public NetworkConnectionEvent Authenticated = new NetworkConnectionEvent();
        public UnityEvent Disconnected = new UnityEvent();

        /// <summary>
        /// The NetworkConnection object this client is using.
        /// </summary>
        public NetworkConnectionToServer Connection { get; internal set; }

        /// <summary>
        /// NetworkIdentity of the localPlayer
        /// </summary>
        public NetworkIdentity LocalPlayer { get; private set; }

        internal ConnectState connectState = ConnectState.None;

        /// <summary>
        /// active is true while a client is connecting/connected
        /// (= while the network is active)
        /// </summary>
        public bool Active => connectState == ConnectState.Connecting || connectState == ConnectState.Connected;

        /// <summary>
        /// This gives the current connection status of the client.
        /// </summary>
        public bool IsConnected => connectState == ConnectState.Connected;

        /// <summary>
        /// List of prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.</para>
        /// </summary>
        public List<GameObject> spawnPrefabs = new List<GameObject>();

        readonly Dictionary<uint, NetworkIdentity> spawned = new Dictionary<uint, NetworkIdentity>();

        public readonly NetworkTime Time = new NetworkTime();

        bool isSpawnFinished;

        /// <summary>
        /// Returns true when a client's connection has been set to ready.
        /// <para>A client that is ready recieves state updates from the server, while a client that is not ready does not. This useful when the state of the game is not normal, such as a scene change or end-of-game.</para>
        /// <para>This is read-only. To change the ready state of a client, use ClientScene.Ready(). The server is able to set the ready state of clients using NetworkServer.SetClientReady(), NetworkServer.SetClientNotReady() and NetworkServer.SetAllClientsNotReady().</para>
        /// <para>This is done when changing scenes so that clients don't receive state update messages during scene loading.</para>
        /// </summary>
        public bool ready { get; internal set; }

        /// <summary>
        /// This is a dictionary of the prefabs that are registered on the client with ClientScene.RegisterPrefab().
        /// <para>The key to the dictionary is the prefab asset Id.</para>
        /// </summary>
        private readonly Dictionary<Guid, GameObject> prefabs = new Dictionary<Guid, GameObject>();

        /// <summary>
        /// This is dictionary of the disabled NetworkIdentity objects in the scene that could be spawned by messages from the server.
        /// <para>The key to the dictionary is the NetworkIdentity sceneId.</para>
        /// </summary>
        public readonly Dictionary<ulong, NetworkIdentity> spawnableObjects = new Dictionary<ulong, NetworkIdentity>();

        /// <summary>
        /// List of all objects spawned in this client
        /// </summary>
        public Dictionary<uint, NetworkIdentity> Spawned
        {
            get
            {
                // if we are in host mode,  the list of spawned object is the same as the server list
                if (hostServer != null)
                    return hostServer.spawned;
                else
                    return spawned;
            }
        }

        /// <summary>
        /// The host server
        /// </summary>
        NetworkServer hostServer;

        /// <summary>
        /// NetworkClient can connect to local server in host mode too
        /// </summary>
        public bool IsLocalClient => hostServer != null;

        void Start()
        {
            InitializeAuthEvents();

            Application.quitting += Shutdown;
        }

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="address"></param>
        public async Task ConnectAsync(string serverIp)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect: " + serverIp);

            Transport.activeTransport.enabled = true;
            InitializeTransportHandlers();
            RegisterSpawnPrefabs();

            connectState = ConnectState.Connecting;
            await Transport.activeTransport.ClientConnectAsync(serverIp);

            // setup all the handlers
            Connection = new NetworkConnectionToServer();
            RegisterMessageHandlers(Connection);
            OnConnected();
        }

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="uri">Address of the server to connect to</param>
        public async Task ConnectAsync(Uri uri)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect: " + uri);

            Transport.activeTransport.enabled = true;
            InitializeTransportHandlers();
            RegisterSpawnPrefabs();

            connectState = ConnectState.Connecting;
            await Transport.activeTransport.ClientConnectAsync(uri);

            // setup all the handlers
            Connection = new NetworkConnectionToServer();
            RegisterMessageHandlers(Connection);
            OnConnected();
        }

        internal void ConnectHost(NetworkServer server)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect Host to Server");
            connectState = ConnectState.Connected;

            // create local connection objects and connect them
            (ULocalConnectionToServer connectionToServer, ULocalConnectionToClient connectionToClient)
                = ULocalConnectionToClient.CreateLocalConnections();

            Connection = connectionToServer;
            RegisterHostHandlers(Connection);

            // create server connection to local client
            server.SetLocalConnection(this, connectionToClient);

            Connected.Invoke(connectionToServer);
            hostServer = server;
        }

        /// <summary>
        /// connect host mode
        /// </summary>
        internal void ConnectLocalServer(NetworkServer server)
        {
            server.OnConnected(server.localConnection);
        }

        void InitializeTransportHandlers()
        {
            Transport.activeTransport.OnClientDataReceived.AddListener(OnDataReceived);
            Transport.activeTransport.OnClientDisconnected.AddListener(OnDisconnected);
            Transport.activeTransport.OnClientError.AddListener(OnError);
        }

        void InitializeAuthEvents()
        {
            if (authenticator != null)
            {
                authenticator.OnClientAuthenticated += OnAuthenticated;

                Connected.AddListener(authenticator.OnClientAuthenticateInternal);
            }
            else
            {
                // if no authenticator, consider connection as authenticated
                Connected.AddListener(OnAuthenticated);
            }
        }

        void OnError(Exception exception)
        {
            Debug.LogException(exception);
        }

        void OnDisconnected()
        {
            connectState = ConnectState.Disconnected;
            HandleClientDisconnect();
        }

        /// <summary>
        /// client that received the message
        /// </summary>
        /// <remarks>This is a hack, but it is needed to deserialize
        /// gameobjects when processing the message</remarks>
        /// 
        internal static NetworkClient Current { get; set; }

        internal void OnDataReceived(ArraySegment<byte> data, int channelId)
        {
            if (Connection != null)
            {
                Connection.TransportReceive(data, channelId);
            }
            else throw new InvalidOperationException("Skipped Data message handling because connection is null.");
        }

        void OnConnected()
        {
            // reset network time stats
            Time.Reset();

            // the handler may want to send messages to the client
            // thus we should set the connected state before calling the handler
            connectState = ConnectState.Connected;
            Time.UpdateClient(this);
            Connected.Invoke(Connection);
        }

        public void OnAuthenticated(NetworkConnectionToServer conn)
        {
            // set connection to authenticated
            conn.isAuthenticated = true;

            Authenticated?.Invoke(conn);
        }

        /// <summary>
        /// Disconnect from server.
        /// <para>The disconnect message will be invoked.</para>
        /// </summary>
        public void Disconnect()
        {
            // local or remote connection?
            if (IsLocalClient)
            {
                if (IsConnected)
                {
                    hostServer.Disconnected.Invoke(hostServer.localConnection);
                }
                hostServer.RemoveLocalConnection();
            }
            else
            {
                if (Connection != null)
                {
                    Connection.Disconnect();
                    RemoveTransportHandlers();
                }
            }
            // set connectState as Disconnected after, otherwise Disconnected event is never raised
            connectState = ConnectState.Disconnected;

            HandleClientDisconnect();
        }

        void RemoveTransportHandlers()
        {
            // so that we don't register them more than once
            Transport.activeTransport.OnClientDataReceived.RemoveListener(OnDataReceived);
            Transport.activeTransport.OnClientDisconnected.RemoveListener(OnDisconnected);
            Transport.activeTransport.OnClientError.RemoveListener(OnError);
        }

        /// <summary>
        /// This sends a network message with a message Id to the server. This message is sent on channel zero, which by default is the reliable channel.
        /// <para>The message must be an instance of a class derived from MessageBase.</para>
        /// <para>The message id passed to Send() is used to identify the handler function to invoke on the server when the message is received.</para>
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="message"></param>
        /// <param name="channelId"></param>
        /// <returns>True if message was sent.</returns>
        public bool Send<T>(T message, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            if (Connection != null)
            {
                if (connectState != ConnectState.Connected)
                {
                    Debug.LogError("NetworkClient Send when not connected to a server");
                    return false;
                }
                return Connection.Send(message, channelId);
            }
            Debug.LogError("NetworkClient Send with no connection");
            return false;
        }

        internal void Update()
        {
            // local connection?
            if (Connection is ULocalConnectionToServer localConnection)
            {
                localConnection.Update();
            }
            // remote connection?
            else
            {
                // only update things while connected
                if (Active && connectState == ConnectState.Connected)
                {
                    Time.UpdateClient(this);
                }
            }
        }

        internal void RegisterHostHandlers(NetworkConnection connection)
        {
            connection.RegisterHandler<ObjectDestroyMessage>(OnHostClientObjectDestroy);
            connection.RegisterHandler<ObjectHideMessage>(OnHostClientObjectHide);
            connection.RegisterHandler<NetworkPongMessage>(msg => { }, false);
            connection.RegisterHandler<SpawnMessage>(OnHostClientSpawn);
            // host mode reuses objects in the server
            // so we don't need to spawn them
            connection.RegisterHandler<ObjectSpawnStartedMessage>(msg => { });
            connection.RegisterHandler<ObjectSpawnFinishedMessage>(msg => { });
            connection.RegisterHandler<UpdateVarsMessage>(msg => { });
            connection.RegisterHandler<RpcMessage>(OnRpcMessage);
            connection.RegisterHandler<SyncEventMessage>(OnSyncEventMessage);
        }

        internal void RegisterMessageHandlers(NetworkConnection connection)
        {
            connection.RegisterHandler<ObjectDestroyMessage>(OnObjectDestroy);
            connection.RegisterHandler<ObjectHideMessage>(OnObjectHide);
            connection.RegisterHandler<NetworkPongMessage>(Time.OnClientPong, false);
            connection.RegisterHandler<SpawnMessage>(OnSpawn);
            connection.RegisterHandler<ObjectSpawnStartedMessage>(OnObjectSpawnStarted);
            connection.RegisterHandler<ObjectSpawnFinishedMessage>(OnObjectSpawnFinished);
            connection.RegisterHandler<UpdateVarsMessage>(OnUpdateVarsMessage);
            connection.RegisterHandler<RpcMessage>(OnRpcMessage);
            connection.RegisterHandler<SyncEventMessage>(OnSyncEventMessage);
        }

        /// <summary>
        /// Shut down a client.
        /// <para>This should be done when a client is no longer going to be used.</para>
        /// </summary>
        public void Shutdown()
        {
            if (LogFilter.Debug) Debug.Log("Shutting down client.");

            ClearSpawners();
            DestroyAllClientObjects();
            Connection = null;
            ready = false;
            isSpawnFinished = false;

            connectState = ConnectState.None;

            if (authenticator != null)
                authenticator.OnClientAuthenticated -= OnAuthenticated;

            // disconnect the client connection.
            // we do NOT call Transport.Shutdown, because someone only called
            // NetworkClient.Shutdown. we can't assume that the server is
            // supposed to be shut down too!
            Transport.activeTransport.ClientDisconnect();
        }

        static bool ConsiderForSpawning(NetworkIdentity identity)
        {
            // not spawned yet, not hidden, etc.?
            return !identity.gameObject.activeSelf &&
                   identity.gameObject.hideFlags != HideFlags.NotEditable &&
                   identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                   identity.sceneId != 0;
        }

        /// <summary>
        /// Removes the player from the game.
        /// </summary>
        /// <returns>True if succcessful</returns>
        public bool RemovePlayer()
        {
            if (LogFilter.Debug) Debug.Log("ClientScene.RemovePlayer() called with connection [" + Connection + "]");

            if (Connection.Identity != null)
            {
                Connection.Send(new RemovePlayerMessage());

                Destroy(Connection.Identity.gameObject);

                Connection.Identity = null;
                LocalPlayer = null;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Signal that the client connection is ready to enter the game.
        /// <para>This could be for example when a client enters an ongoing game and has finished loading the current scene. The server should respond to the SYSTEM_READY event with an appropriate handler which instantiates the players object for example.</para>
        /// </summary>
        /// <param name="conn">The client connection which is ready.</param>
        /// <returns>True if succcessful</returns>
        public bool Ready(NetworkConnectionToServer conn)
        {
            if (ready)
            {
                Debug.LogError("A connection has already been set as ready. There can only be one.");
                return false;
            }

            if (LogFilter.Debug) Debug.Log("ClientScene.Ready() called with connection [" + conn + "]");

            if (conn != null)
            {
                // Set these before sending the ReadyMessage, otherwise host client
                // will fail in InternalAddPlayer with null readyConnection.
                ready = true;
                Connection = conn;
                Connection.isReady = true;

                // Tell server we're ready to have a player object spawned
                conn.Send(new ReadyMessage());

                return true;
            }
            Debug.LogError("Ready() called with invalid connection object: conn=null");
            return false;
        }

        // this is called from message handler for Owner message
        internal void InternalAddPlayer(NetworkIdentity identity)
        {
            if (LogFilter.Debug) Debug.LogWarning("ClientScene.InternalAddPlayer");

            // NOTE: It can be "normal" when changing scenes for the player to be destroyed and recreated.
            // But, the player structures are not cleaned up, we'll just replace the old player
            LocalPlayer = identity;

            if (Connection != null)
            {
                Connection.Identity = identity;
            }
            else
            {
                Debug.LogWarning("No ready connection found for setting player controller during InternalAddPlayer");
            }
        }

        internal void HandleClientDisconnect()
        {
            DestroyAllClientObjects();
            ready = false;
            Connection = null;

            Disconnected.Invoke();
        }

        /// <summary>
        /// Call this after loading/unloading a scene in the client after connection to register the spawnable objects
        /// </summary>
        public void PrepareToSpawnSceneObjects()
        {
            // add all unspawned NetworkIdentities to spawnable objects
            spawnableObjects.Clear();
            IEnumerable<NetworkIdentity> sceneObjects =
                Resources.FindObjectsOfTypeAll<NetworkIdentity>()
                               .Where(ConsiderForSpawning);

            foreach (NetworkIdentity obj in sceneObjects)
            {
                spawnableObjects.Add(obj.sceneId, obj);
            }
        }

        NetworkIdentity SpawnSceneObject(ulong sceneId)
        {
            if (spawnableObjects.TryGetValue(sceneId, out NetworkIdentity identity))
            {
                spawnableObjects.Remove(sceneId);
                return identity;
            }
            Debug.LogWarning("Could not find scene object with sceneid:" + sceneId.ToString("X"));
            return null;
        }


        #region Spawn Prefabs
        private void RegisterSpawnPrefabs()
        {
            for (int i = 0; i < spawnPrefabs.Count; i++)
            {
                GameObject prefab = spawnPrefabs[i];
                if (prefab != null)
                {
                    RegisterPrefab(prefab);
                }
            }
        }

        /// <summary>
        /// Find the registered prefab for this asset id.
        /// Useful for debuggers
        /// </summary>
        /// <param name="assetId">asset id of the prefab</param>
        /// <param name="prefab">the prefab gameobject</param>
        /// <returns>true if prefab was registered</returns>
        public bool GetPrefab(Guid assetId, out GameObject prefab)
        {
            prefab = null;
            return assetId != Guid.Empty &&
                   prefabs.TryGetValue(assetId, out prefab) && prefab != null;
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The NetworkManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of NetworkAssetIds and prefab references.</para>
        /// </summary>
        /// <param name="prefab">A Prefab that will be spawned.</param>
        /// <param name="newAssetId">An assetId to be assigned to this prefab. This allows a dynamically created game object to be registered for an already known asset Id.</param>
        public void RegisterPrefab(GameObject prefab, Guid newAssetId)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity)
            {
                identity.assetId = newAssetId;

                if (LogFilter.Debug) Debug.Log("Registering prefab '" + prefab.name + "' as asset:" + identity.assetId);
                prefabs[identity.assetId] = prefab;
            }
            else
            {
                Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
            }
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The NetworkManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of NetworkAssetIds and prefab references.</para>
        /// </summary>
        /// <param name="prefab">A Prefab that will be spawned.</param>
        public void RegisterPrefab(GameObject prefab)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity)
            {
                if (LogFilter.Debug) Debug.Log("Registering prefab '" + prefab.name + "' as asset:" + identity.assetId);
                prefabs[identity.assetId] = prefab;

                NetworkIdentity[] identities = prefab.GetComponentsInChildren<NetworkIdentity>();
                if (identities.Length > 1)
                {
                    Debug.LogWarning("The prefab '" + prefab.name +
                                     "' has multiple NetworkIdentity components. There can only be one NetworkIdentity on a prefab, and it must be on the root object.");
                }
            }
            else
            {
                Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
            }
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The NetworkManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of NetworkAssetIds and prefab references.</para>
        /// </summary>
        /// <param name="prefab">A Prefab that will be spawned.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            RegisterPrefab(prefab, msg => spawnHandler(msg.position, msg.assetId), unspawnHandler);
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The NetworkManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of NetworkAssetIds and prefab references.</para>
        /// </summary>
        /// <param name="prefab">A Prefab that will be spawned.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterPrefab(GameObject prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
                return;
            }

            if (spawnHandler == null || unspawnHandler == null)
            {
                Debug.LogError("RegisterPrefab custom spawn function null for " + identity.assetId);
                return;
            }

            if (identity.assetId == Guid.Empty)
            {
                Debug.LogError("RegisterPrefab game object " + prefab.name + " has no prefab. Use RegisterSpawnHandler() instead?");
                return;
            }

            if (LogFilter.Debug) Debug.Log("Registering custom prefab '" + prefab.name + "' as asset:" + identity.assetId + " " + spawnHandler.GetMethodName() + "/" + unspawnHandler.GetMethodName());

            spawnHandlers[identity.assetId] = spawnHandler;
            unspawnHandlers[identity.assetId] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn prefab that was setup with ClientScene.RegisterPrefab.
        /// </summary>
        /// <param name="prefab">The prefab to be removed from registration.</param>
        public void UnregisterPrefab(GameObject prefab)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                Debug.LogError("Could not unregister '" + prefab.name + "' since it contains no NetworkIdentity component");
                return;
            }
            spawnHandlers.Remove(identity.assetId);
            unspawnHandlers.Remove(identity.assetId);
        }

        #endregion

        #region Spawn Handler

        /// <summary>
        /// This is an advanced spawning function that registers a custom assetId with the UNET spawning system.
        /// <para>This can be used to register custom spawning methods for an assetId - instead of the usual method of registering spawning methods for a prefab. This should be used when no prefab exists for the spawned objects - such as when they are constructed dynamically at runtime from configuration data.</para>
        /// </summary>
        /// <param name="assetId">Custom assetId string.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterSpawnHandler(Guid assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            RegisterSpawnHandler(assetId, msg => spawnHandler(msg.position, msg.assetId), unspawnHandler);
        }

        /// <summary>
        /// This is an advanced spawning function that registers a custom assetId with the UNET spawning system.
        /// <para>This can be used to register custom spawning methods for an assetId - instead of the usual method of registering spawning methods for a prefab. This should be used when no prefab exists for the spawned objects - such as when they are constructed dynamically at runtime from configuration data.</para>
        /// </summary>
        /// <param name="assetId">Custom assetId string.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterSpawnHandler(Guid assetId, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            if (spawnHandler == null || unspawnHandler == null)
            {
                Debug.LogError("RegisterSpawnHandler custom spawn function null for " + assetId);
                return;
            }

            if (LogFilter.Debug) Debug.Log("RegisterSpawnHandler asset '" + assetId + "' " + spawnHandler.GetMethodName() + "/" + unspawnHandler.GetMethodName());

            spawnHandlers[assetId] = spawnHandler;
            unspawnHandlers[assetId] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn handler function that was registered with ClientScene.RegisterHandler().
        /// </summary>
        /// <param name="assetId">The assetId for the handler to be removed for.</param>
        public void UnregisterSpawnHandler(Guid assetId)
        {
            spawnHandlers.Remove(assetId);
            unspawnHandlers.Remove(assetId);
        }

        /// <summary>
        /// This clears the registered spawn prefabs and spawn handler functions for this client.
        /// </summary>
        public void ClearSpawners()
        {
            prefabs.Clear();
            spawnHandlers.Clear();
            unspawnHandlers.Clear();
        }

        #endregion

        void UnSpawn(NetworkIdentity identity)
        {
            Guid assetId = identity.assetId;

            identity.NetworkDestroy();
            if (unspawnHandlers.TryGetValue(assetId, out UnSpawnDelegate handler) && handler != null)
            {
                handler(identity.gameObject);
            }
            else if (identity.sceneId == 0)
            {
                Destroy(identity.gameObject);
            }
            else
            {
                identity.MarkForReset();
                identity.gameObject.SetActive(false);
                spawnableObjects[identity.sceneId] = identity;
            }
        }

        /// <summary>
        /// Destroys all networked objects on the client.
        /// <para>This can be used to clean up when a network connection is closed.</para>
        /// </summary>
        public void DestroyAllClientObjects()
        {
            foreach (NetworkIdentity identity in Spawned.Values)
            {
                if (identity != null && identity.gameObject != null)
                {
                    UnSpawn(identity);
                }
            }
            Spawned.Clear();
        }

        void ApplySpawnPayload(NetworkIdentity identity, SpawnMessage msg)
        {
            identity.Reset();

            if (msg.assetId != Guid.Empty)
                identity.assetId = msg.assetId;

            if (!identity.gameObject.activeSelf)
            {
                identity.gameObject.SetActive(true);
            }

            // apply local values for VR support
            identity.transform.localPosition = msg.position;
            identity.transform.localRotation = msg.rotation;
            identity.transform.localScale = msg.scale;
            identity.hasAuthority = msg.isOwner;
            identity.netId = msg.netId;
            identity.server = hostServer;
            identity.client = this;

            if (msg.isLocalPlayer)
                InternalAddPlayer(identity);

            // deserialize components if any payload
            // (Count is 0 if there were no components)
            if (msg.payload.Count > 0)
            {
                using (PooledNetworkReader payloadReader = NetworkReaderPool.GetReader(msg.payload))
                {
                    identity.OnDeserializeAllSafely(payloadReader, true);
                }
            }

            Spawned[msg.netId] = identity;

            // objects spawned as part of initial state are started on a second pass
            if (isSpawnFinished)
            {
                identity.NotifyAuthority();
                identity.StartClient();
                CheckForLocalPlayer(identity);
            }
        }

        internal void OnSpawn(SpawnMessage msg)
        {
            if (msg.assetId == Guid.Empty && msg.sceneId == 0)
            {
                Debug.LogError("OnObjSpawn netId: " + msg.netId + " has invalid asset Id");
                return;
            }
            if (LogFilter.Debug) Debug.Log($"Client spawn handler instantiating netId={msg.netId} assetID={msg.assetId} sceneId={msg.sceneId} pos={msg.position}");

            // was the object already spawned?
            NetworkIdentity identity = GetExistingObject(msg.netId);

            if (identity == null)
            {
                identity = msg.sceneId == 0 ? SpawnPrefab(msg) : SpawnSceneObject(msg);
            }

            if (identity == null)
            {
                Debug.LogError($"Could not spawn assetId={msg.assetId} scene={msg.sceneId} netId={msg.netId}");
                return;
            }

            ApplySpawnPayload(identity, msg);
        }

        NetworkIdentity GetExistingObject(uint netid)
        {
            Spawned.TryGetValue(netid, out NetworkIdentity localObject);
            return localObject;
        }

        NetworkIdentity SpawnPrefab(SpawnMessage msg)
        {
            if (GetPrefab(msg.assetId, out GameObject prefab))
            {
                GameObject obj = Object.Instantiate(prefab, msg.position, msg.rotation);
                if (LogFilter.Debug)
                {
                    Debug.Log("Client spawn handler instantiating [netId:" + msg.netId + " asset ID:" + msg.assetId + " pos:" + msg.position + " rotation: " + msg.rotation + "]");
                }

                return obj.GetComponent<NetworkIdentity>();
            }
            if (spawnHandlers.TryGetValue(msg.assetId, out SpawnHandlerDelegate handler))
            {
                GameObject obj = handler(msg);
                if (obj == null)
                {
                    Debug.LogWarning("Client spawn handler for " + msg.assetId + " returned null");
                    return null;
                }
                return obj.GetComponent<NetworkIdentity>();
            }
            Debug.LogError("Failed to spawn server object, did you forget to add it to the NetworkManager? assetId=" + msg.assetId + " netId=" + msg.netId);
            return null;
        }

        NetworkIdentity SpawnSceneObject(SpawnMessage msg)
        {
            NetworkIdentity spawnedId = SpawnSceneObject(msg.sceneId);
            if (spawnedId == null)
            {
                Debug.LogError("Spawn scene object not found for " + msg.sceneId.ToString("X") + " SpawnableObjects.Count=" + spawnableObjects.Count);

                // dump the whole spawnable objects dict for easier debugging
                if (LogFilter.Debug)
                {
                    foreach (KeyValuePair<ulong, NetworkIdentity> kvp in spawnableObjects)
                        Debug.Log("Spawnable: SceneId=" + kvp.Key + " name=" + kvp.Value.name);
                }
            }

            if (LogFilter.Debug) Debug.Log("Client spawn for [netId:" + msg.netId + "] [sceneId:" + msg.sceneId + "] obj:" + spawnedId);
            return spawnedId;
        }

        internal void OnObjectSpawnStarted(ObjectSpawnStartedMessage _)
        {
            if (LogFilter.Debug) Debug.Log("SpawnStarted");

            PrepareToSpawnSceneObjects();
            isSpawnFinished = false;
        }

        internal void OnObjectSpawnFinished(ObjectSpawnFinishedMessage _)
        {
            if (LogFilter.Debug) Debug.Log("SpawnFinished");

            // paul: Initialize the objects in the same order as they were initialized
            // in the server.   This is important if spawned objects
            // use data from scene objects
            foreach (NetworkIdentity identity in Spawned.Values.OrderBy(uv => uv.netId))
            {
                identity.NotifyAuthority();
                identity.StartClient();
                CheckForLocalPlayer(identity);
            }
            isSpawnFinished = true;
        }

        internal void OnObjectHide(ObjectHideMessage msg)
        {
            DestroyObject(msg.netId);
        }

        internal void OnObjectDestroy(ObjectDestroyMessage msg)
        {
            DestroyObject(msg.netId);
        }

        void DestroyObject(uint netId)
        {
            if (LogFilter.Debug) Debug.Log("ClientScene.OnObjDestroy netId:" + netId);

            if (Spawned.TryGetValue(netId, out NetworkIdentity localObject) && localObject != null)
            {
                UnSpawn(localObject);
                Spawned.Remove(netId);
            }
            else
            {
                if (LogFilter.Debug) Debug.LogWarning("Did not find target for destroy message for " + netId);
            }
        }

        internal void OnHostClientObjectDestroy(ObjectDestroyMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("ClientScene.OnLocalObjectObjDestroy netId:" + msg.netId);

            Spawned.Remove(msg.netId);
        }

        internal void OnHostClientObjectHide(ObjectHideMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("ClientScene::OnLocalObjectObjHide netId:" + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnSetHostVisibility(false);
            }
        }

        internal void OnHostClientSpawn(SpawnMessage msg)
        {
            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                if (msg.isLocalPlayer)
                    InternalAddPlayer(localObject);

                localObject.hasAuthority = msg.isOwner;
                localObject.NotifyAuthority();
                localObject.StartClient();
                localObject.OnSetHostVisibility(true);
                CheckForLocalPlayer(localObject);
            }
        }

        internal void OnUpdateVarsMessage(UpdateVarsMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("ClientScene.OnUpdateVarsMessage " + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                    localObject.OnDeserializeAllSafely(networkReader, false);
            }
            else
            {
                Debug.LogWarning("Did not find target for sync message for " + msg.netId + " . Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message.");
            }
        }

        internal void OnRpcMessage(RpcMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("ClientScene.OnRPCMessage hash:" + msg.functionHash + " netId:" + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                    identity.HandleRPC(msg.componentIndex, msg.functionHash, networkReader);
            }
        }

        internal void OnSyncEventMessage(SyncEventMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("ClientScene.OnSyncEventMessage " + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                    identity.HandleSyncEvent(msg.componentIndex, msg.functionHash, networkReader);
            }
            else
            {
                Debug.LogWarning("Did not find target for SyncEvent message for " + msg.netId);
            }
        }

        void CheckForLocalPlayer(NetworkIdentity identity)
        {
            if (identity == LocalPlayer)
            {
                // Set isLocalPlayer to true on this NetworkIdentity and trigger OnStartLocalPlayer in all scripts on the same GO
                identity.connectionToServer = Connection;
                identity.StartLocalPlayer();

                if (LogFilter.Debug) Debug.Log("ClientScene.OnOwnerMessage - player=" + identity.name);
            }
        }
    }
}
