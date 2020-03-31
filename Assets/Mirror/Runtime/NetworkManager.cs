using System;
using Mirror.Tcp;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirror
{

    /// <summary>
    /// Enumeration of methods of current Network Manager state at runtime.
    /// </summary>
    public enum NetworkManagerMode { Offline, ServerOnly, ClientOnly, Host }

    [AddComponentMenu("Network/NetworkManager")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkManager.html")]
    [RequireComponent(typeof(NetworkServer))]
    [RequireComponent(typeof(NetworkClient))]
    [DisallowMultipleComponent]
    public class NetworkManager : MonoBehaviour
    {
        /// <summary>
        /// A flag to control whether the NetworkManager object is destroyed when the scene changes.
        /// <para>This should be set if your game has a single NetworkManager that exists for the lifetime of the process. If there is a NetworkManager in each scene, then this should not be set.</para>
        /// </summary>
        [Header("Configuration")]
        [FormerlySerializedAs("m_DontDestroyOnLoad")]
        [Tooltip("Should the Network Manager object be persisted through scene changes?")]
        public bool dontDestroyOnLoad = true;

        /// <summary>
        /// Automatically invoke StartServer()
        /// <para>If the application is a Server Build or run with the -batchMode command line arguement, StartServer is automatically invoked.</para>
        /// </summary>
        [Tooltip("Should the server auto-start when the game is started in a headless build?")]
        public bool startOnHeadless = true;

        /// <summary>
        /// Enables verbose debug messages in the console
        /// </summary>
        [FormerlySerializedAs("m_ShowDebugMessages")]
        [Tooltip("This will enable verbose debug messages in the Unity Editor console")]
        public bool showDebugMessages;

        /// <summary>
        /// Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.
        /// </summary>
        [Tooltip("Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.")]
        public int serverTickRate = 30;

        public NetworkServer server;
        public NetworkClient client;

        // transport layer
        [Header("Network Info")]
        [Tooltip("Transport component attached to this object that server and client will use to connect")]
        [SerializeField]
        protected Transport transport;

        /// <summary>
        /// True if the server or client is started and running
        /// <para>This is set True in StartServer / StartClient, and set False in StopServer / StopClient</para>
        /// </summary>
        [NonSerialized]
        public bool isNetworkActive;

        /// <summary>
        /// This is true if the client loaded a new scene when connecting to the server.
        /// <para>This is set before OnClientConnect is called, so it can be checked there to perform different logic if a scene load occurred.</para>
        /// </summary>
        [NonSerialized]
        public bool clientLoadedScene;

        /// <summary>
        /// headless mode detection
        /// </summary>
        public static bool isHeadless => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

        // helper enum to know if we started the networkmanager as server/client/host.
        // -> this is necessary because when StartHost changes server scene to
        //    online scene, FinishLoadScene is called and the host client isn't
        //    connected yet (no need to connect it before server was fully set up).
        //    in other words, we need this to know which mode we are running in
        //    during FinishLoadScene.
        public NetworkManagerMode mode { get; private set; }

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public UnityEvent OnStartHost = new UnityEvent();

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        public UnityEvent OnStopHost = new UnityEvent();

        #region Unity Callbacks

        /// <summary>
        /// virtual so that inheriting classes' OnValidate() can call base.OnValidate() too
        /// </summary>
        public virtual void OnValidate()
        {
            // add transport if there is none yet. makes upgrading easier.
            if (transport == null)
            {
                // was a transport added yet? if not, add one
                transport = GetComponent<Transport>();
                if (transport == null)
                {
                    transport = gameObject.AddComponent<TcpTransport>();
                    Debug.Log("NetworkManager: added default Transport because there was none yet.");
                }
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added default Transport");
#endif
            }

            // add NetworkServer if there is none yet. makes upgrading easier.
            if (GetComponent<NetworkServer>() == null)
            {
                server = gameObject.AddComponent<NetworkServer>();
                Debug.Log("NetworkManager: added NetworkServer because there was none yet.");
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added NetworkServer");
#endif
            }

            // add NetworkClient if there is none yet. makes upgrading easier.
            if (GetComponent<NetworkClient>() == null)
            {
                client = gameObject.AddComponent<NetworkClient>();
                Debug.Log("NetworkManager: added NetworkClient because there was none yet.");
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added NetworkClient");
#endif
            }
        }

        /// <summary>
        /// virtual so that inheriting classes' Start() can call base.Start() too
        /// </summary>
        public virtual void Start()
        {
            Debug.Log("Thank you for using Mirror! https://mirror-networking.com");

            // Set the networkSceneName to prevent a scene reload
            // if client connection to server fails.
            networkSceneName = null;

            Initialize();

            // setup OnSceneLoaded callback
            SceneManager.sceneLoaded += OnSceneLoaded;

            // headless mode? then start the server
            // can't do this in Awake because Awake is for initialization.
            // some transports might not be ready until Start.
            //
            // (tick rate is applied in StartServer!)
            if (isHeadless && startOnHeadless)
            {
                StartServer();
            }
        }

        // NetworkIdentity.UNetStaticUpdate is called from UnityEngine while LLAPI network is active.
        // If we want TCP then we need to call it manually. Probably best from NetworkManager, although this means that we can't use NetworkServer/NetworkClient without a NetworkManager invoking Update anymore.
        /// <summary>
        /// virtual so that inheriting classes' LateUpdate() can call base.LateUpdate() too
        /// </summary>
        public virtual void LateUpdate()
        {
            UpdateScene();
        }

        #endregion

        #region Start & Stop

        // full server setup code, without spawning objects yet
        void SetupServer()
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager SetupServer");

            ConfigureServerFrameRate();

            // start listening to network connections
            server.Listen();

            isNetworkActive = true;
        }

        /// <summary>
        /// This starts a new server.
        /// <para>This uses the networkPort property as the listen port.</para>
        /// </summary>
        /// <returns></returns>
        public void StartServer()
        {
            mode = NetworkManagerMode.ServerOnly;

            // StartServer is inherently ASYNCHRONOUS (=doesn't finish immediately)
            //
            // Here is what it does:
            //   Listen
            //   if onlineScene:
            //       LoadSceneAsync
            //       ...
            //       FinishLoadSceneServerOnly
            //           SpawnObjects
            //   else:
            //       SpawnObjects
            //
            // there is NO WAY to make it synchronous because both LoadSceneAsync
            // and LoadScene do not finish loading immediately. as long as we
            // have the onlineScene feature, it will be asynchronous!

            SetupServer();

            server.SpawnObjects();
        }

        /// <summary>
        /// This starts a network client. It uses the networkAddress and networkPort properties as the address to connect to.
        /// <para>This makes the newly created client connect to the server immediately.</para>
        /// </summary>
        public void StartClient(string serverIp)
        {
            mode = NetworkManagerMode.ClientOnly;

            isNetworkActive = true;
            
            if (string.IsNullOrEmpty(serverIp))
            {
                Debug.LogError("serverIp shouldn't be empty");
                return;
            }
            if (LogFilter.Debug) Debug.Log("NetworkManager StartClient address:" + serverIp);

            _ = client.ConnectAsync(serverIp);
        }

        /// <summary>
        /// This starts a network client. It uses the networkAddress and networkPort properties as the address to connect to.
        /// <para>This makes the newly created client connect to the server immediately.</para>
        /// </summary>
        /// <param name="uri">location of the server to connect to</param>
        public void StartClient(Uri uri)
        {
            mode = NetworkManagerMode.ClientOnly;
            isNetworkActive = true;


            if (LogFilter.Debug) Debug.Log("NetworkManager StartClient address:" + uri);

            _ = client.ConnectAsync(uri);
        }

        /// <summary>
        /// This starts a network "host" - a server and client in the same application.
        /// <para>The client returned from StartHost() is a special "local" client that communicates to the in-process server using a message queue instead of the real network. But in almost all other cases, it can be treated as a normal client.</para>
        /// </summary>
        public void StartHost()
        {
            mode = NetworkManagerMode.Host;

            // StartHost is inherently ASYNCHRONOUS (=doesn't finish immediately)
            //
            // Here is what it does:
            //   Listen
            //   ConnectHost
            //   if onlineScene:
            //       LoadSceneAsync
            //       ...
            //       FinishLoadSceneHost
            //           FinishStartHost
            //               SpawnObjects
            //               StartHostClient      <= not guaranteed to happen after SpawnObjects if onlineScene is set!
            //                   ClientAuth
            //                       success: server sends changescene msg to client
            //   else:
            //       FinishStartHost
            //
            // there is NO WAY to make it synchronous because both LoadSceneAsync
            // and LoadScene do not finish loading immediately. as long as we
            // have the onlineScene feature, it will be asynchronous!

            // setup server first
            SetupServer();

            // call OnStartHost AFTER SetupServer. this way we can use
            // NetworkServer.Spawn etc. in there too. just like OnStartServer
            // is called after the server is actually properly started.
            OnStartHost.Invoke();

            FinishStartHost();
        }

        // FinishStartHost is guaranteed to be called after the host server was
        // fully started and all the asynchronous StartHost magic is finished
        // (= scene loading), or immediately if there was no asynchronous magic.
        //
        // note: we don't really need FinishStartClient/FinishStartServer. the
        //       host version is enough.
        void FinishStartHost()
        {
            // ConnectHost needs to be called BEFORE SpawnObjects:
            // https://github.com/vis2k/Mirror/pull/1249/
            // -> this sets NetworkServer.localConnection.
            // -> localConnection needs to be set before SpawnObjects because:
            //    -> SpawnObjects calls OnStartServer in all NetworkBehaviours
            //       -> OnStartServer might spawn an object and set [SyncVar(hook="OnColorChanged")] object.color = green;
            //          -> this calls SyncVar.set (generated by Weaver), which has
            //             a custom case for host mode (because host mode doesn't
            //             get OnDeserialize calls, where SyncVar hooks are usually
            //             called):
            //
            //               if (!SyncVarEqual(value, ref color))
            //               {
            //                   if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
            //                   {
            //                       setSyncVarHookGuard(1uL, value: true);
            //                       OnColorChangedHook(value);
            //                       setSyncVarHookGuard(1uL, value: false);
            //                   }
            //                   SetSyncVar(value, ref color, 1uL);
            //               }
            //
            //          -> localClientActive needs to be true, otherwise the hook
            //             isn't called in host mode!
            //

            // TODO call this after spawnobjects and worry about the syncvar hook fix later?
            client.ConnectHost(server);

            // server scene was loaded. now spawn all the objects
            server.SpawnObjects();

            // connect client and call OnStartClient AFTER server scene was
            // loaded and all objects were spawned.
            // DO NOT do this earlier. it would cause race conditions where a
            // client will do things before the server is even fully started.
            if (LogFilter.Debug) Debug.Log("StartHostClient called");
            StartHostClient();
        }

        void StartHostClient()
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager ConnectLocalClient");

            server.ActivateHostScene();
            client.ConnectLocalServer(server);

            RegisterClientMessages(client.Connection);
        }

        /// <summary>
        /// This stops both the client and the server that the manager is using.
        /// </summary>
        public void StopHost()
        {
            OnStopHost.Invoke();
            StopClient();
            StopServer();
        }

        /// <summary>
        /// Stops the server that the manager is using.
        /// </summary>
        public void StopServer()
        {
            if (!server.active)
                return;

            if (LogFilter.Debug) Debug.Log("NetworkManager StopServer");
            isNetworkActive = false;
            server.Shutdown();

            // set offline mode BEFORE changing scene so that FinishStartScene
            // doesn't think we need initialize anything.
            mode = NetworkManagerMode.Offline;
        }

        /// <summary>
        /// Stops the client that the manager is using.
        /// </summary>
        public void StopClient()
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager StopClient");
            isNetworkActive = false;
            clientLoadedScene = false;

            // shutdown client
            client.Disconnect();
            client.Shutdown();

            // set offline mode BEFORE changing scene so that FinishStartScene
            // doesn't think we need initialize anything.
            mode = NetworkManagerMode.Offline;

        }

        /// <summary>
        /// Set the frame rate for a headless server.
        /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
        /// </summary>
        public virtual void ConfigureServerFrameRate()
        {
            // set a fixed tick rate instead of updating as often as possible
            // * if not in Editor (it doesn't work in the Editor)
            // * if not in Host mode
#if !UNITY_EDITOR
            if (!client.active && isHeadless)
            {
                Application.targetFrameRate = serverTickRate;
                Debug.Log("Server Tick Rate set to: " + Application.targetFrameRate + " Hz.");
            }
#endif
        }

        void Initialize()
        {
            LogFilter.Debug = showDebugMessages;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            Transport.activeTransport = transport;

            // subscribe to the server
            if (server != null)
                server.Authenticated.AddListener(OnServerAuthenticated);


            // subscribe to the client
            if (client != null)
                client.Authenticated.AddListener(OnClientAuthenticated);
        }

        /// <summary>
        /// virtual so that inheriting classes' OnDestroy() can call base.OnDestroy() too
        /// </summary>
        public virtual void OnDestroy()
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager destroyed");
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// The name of the current network scene.
        /// </summary>
        /// <remarks>
        /// <para>This is populated if the NetworkManager is doing scene management. This should not be changed directly. Calls to ServerChangeScene() cause this to change. New clients that connect to a server will automatically load this scene.</para>
        /// <para>This is used to make sure that all scene changes are initialized by Mirror.</para>
        /// <para>Loading a scene manually wont set networkSceneName, so Mirror would still load it again on start.</para>
        /// </remarks>
        public string networkSceneName = "";

        public AsyncOperation loadingSceneAsync;

        /// <summary>
        /// This causes the server to switch scenes and sets the networkSceneName.
        /// <para>Clients that connect to this server will automatically switch to this scene. This is called autmatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
        /// </summary>
        /// <param name="newSceneName"></param>
        public virtual void ServerChangeScene(string newSceneName)
        {
            if (string.IsNullOrEmpty(newSceneName))
            {
                Debug.LogError("ServerChangeScene empty scene name");
                return;
            }

            if (LogFilter.Debug) Debug.Log("ServerChangeScene " + newSceneName);
            server.SetAllClientsNotReady();
            networkSceneName = newSceneName;

            // Let server prepare for scene change
            OnServerChangeScene(newSceneName);

            // Suspend the server's transport while changing scenes
            // It will be re-enabled in FinishScene.
            Transport.activeTransport.enabled = false;
            loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);

            // notify all clients about the new scene
            server.SendToAll(new SceneMessage { sceneName = newSceneName });
        }

        internal void ClientChangeScene(string newSceneName, SceneOperation sceneOperation = SceneOperation.Normal, bool customHandling = false)
        {
            if (string.IsNullOrEmpty(newSceneName))
            {
                Debug.LogError("ClientChangeScene empty scene name");
                return;
            }

            if (LogFilter.Debug) Debug.Log("ClientChangeScene newSceneName:" + newSceneName + " networkSceneName:" + networkSceneName);

            // vis2k: pause message handling while loading scene. otherwise we will process messages and then lose all
            // the state as soon as the load is finishing, causing all kinds of bugs because of missing state.
            // (client may be null after StopClient etc.)
            if (LogFilter.Debug) Debug.Log("ClientChangeScene: pausing handlers while scene is loading to avoid data loss after scene was loaded.");
            Transport.activeTransport.enabled = false;
            // Let client prepare for scene change
            OnClientChangeScene(newSceneName, sceneOperation, customHandling);

            // scene handling will happen in overrides of OnClientChangeScene and/or OnClientSceneChanged
            if (customHandling)
            {
                FinishLoadScene();
                return;
            }

            switch (sceneOperation)
            {
                case SceneOperation.Normal:
                    loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
                    break;
                case SceneOperation.LoadAdditive:
                    if (!SceneManager.GetSceneByName(newSceneName).IsValid())
                        loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
                    else
                        Debug.LogWarningFormat("Scene {0} is already loaded", newSceneName);
                    break;
                case SceneOperation.UnloadAdditive:
                    if (SceneManager.GetSceneByName(newSceneName).IsValid())
                    {
                        if (SceneManager.GetSceneByName(newSceneName) != null)
                            loadingSceneAsync = SceneManager.UnloadSceneAsync(newSceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                    }
                    else
                        Debug.LogWarning("Cannot unload the active scene with UnloadAdditive operation");
                    break;
            }

            // don't change the client's current networkSceneName when loading additive scene content
            if (sceneOperation == SceneOperation.Normal)
                networkSceneName = newSceneName;
        }

        // support additive scene loads:
        //   NetworkScenePostProcess disables all scene objects on load, and
        //   * NetworkServer.SpawnObjects enables them again on the server when
        //     calling OnStartServer
        //   * ClientScene.PrepareToSpawnSceneObjects enables them again on the
        //     client after the server sends ObjectSpawnStartedMessage to client
        //     in SpawnObserversForConnection. this is only called when the
        //     client joins, so we need to rebuild scene objects manually again
        // TODO merge this with FinishLoadScene()?
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                if (server.active)
                {
                    // TODO only respawn the server objects from that scene later!
                    server.SpawnObjects();
                    if (LogFilter.Debug) Debug.Log("Respawned Server objects after additive scene load: " + scene.name);
                }
                if (client.Active)
                {
                    client.PrepareToSpawnSceneObjects();
                    if (LogFilter.Debug) Debug.Log("Rebuild Client spawnableObjects after additive scene load: " + scene.name);
                }
            }
        }

        void UpdateScene()
        {
            if (loadingSceneAsync != null && loadingSceneAsync.isDone)
            {
                if (LogFilter.Debug) Debug.Log("ClientChangeScene done readyCon:" + client.Connection);
                FinishLoadScene();
                loadingSceneAsync.allowSceneActivation = true;
                loadingSceneAsync = null;
            }
        }

        void FinishLoadScene()
        {
            // NOTE: this cannot use NetworkClient.allClients[0] - that client may be for a completely different purpose.

            // process queued messages that we received while loading the scene
            if (LogFilter.Debug) Debug.Log("FinishLoadScene: resuming handlers after scene was loading.");
            Transport.activeTransport.enabled = true;

            // host mode?
            if (mode == NetworkManagerMode.Host)
            {
                FinishLoadSceneHost();
            }
            // server-only mode?
            else if (mode == NetworkManagerMode.ServerOnly)
            {
                FinishLoadSceneServerOnly();
            }
            // client-only mode?
            else if (mode == NetworkManagerMode.ClientOnly)
            {
                FinishLoadSceneClientOnly();
            }
        }

        // finish load scene part for host mode. makes code easier and is
        // necessary for FinishStartHost later.
        // (the 3 things have to happen in that exact order)
        void FinishLoadSceneHost()
        {
            // debug message is very important. if we ever break anything then
            // it's very obvious to notice.
            if (LogFilter.Debug) Debug.Log("Finished loading scene in host mode.");

            if (client.Connection != null)
            {
                client.OnAuthenticated(client.Connection);
                clientLoadedScene = true;
                client.Connection = null;
            }

            FinishStartHost();

            // call OnServerSceneChanged
            OnServerSceneChanged(networkSceneName);

            if (client.IsConnected)
            {
                // let client know that we changed scene
                OnClientSceneChanged(client.Connection);
            }
        }

        // finish load scene part for client-only. makes code easier and is
        // necessary for FinishStartClient later.
        void FinishLoadSceneClientOnly()
        {
            // debug message is very important. if we ever break anything then
            // it's very obvious to notice.
            if (LogFilter.Debug) Debug.Log("Finished loading scene in client-only mode.");

            if (client.Connection != null)
            {
                client.OnAuthenticated(client.Connection);
                clientLoadedScene = true;
                client.Connection = null;
            }

            if (client.IsConnected)
            {
                OnClientSceneChanged(client.Connection);
            }
        }

        // finish load scene part for server-only. . makes code easier and is
        // necessary for FinishStartServer later.
        void FinishLoadSceneServerOnly()
        {
            // debug message is very important. if we ever break anything then
            // it's very obvious to notice.
            if (LogFilter.Debug) Debug.Log("Finished loading scene in server-only mode.");

            server.SpawnObjects();
            OnServerSceneChanged(networkSceneName);
        }

        #endregion

        #region Server Internal Message Handlers

        void RegisterServerMessages(NetworkConnection connection)
        {
            connection.RegisterHandler<NetworkConnectionToClient, ReadyMessage>(OnServerReadyMessageInternal);
            connection.RegisterHandler<NetworkConnectionToClient, RemovePlayerMessage>(OnServerRemovePlayerMessageInternal);
        }

        // called after successful authentication
        void OnServerAuthenticated(NetworkConnectionToClient conn)
        {
            // a connection has been established,  register for our messages
            RegisterServerMessages(conn);

            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerAuthenticated");

            // proceed with the login handshake by calling OnServerConnect
            if (!string.IsNullOrEmpty(networkSceneName))
            {
                var msg = new SceneMessage() { sceneName = networkSceneName };
                conn.Send(msg);
            }

            OnServerConnect(conn);
        }

        void OnServerReadyMessageInternal(NetworkConnection conn, ReadyMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerReadyMessageInternal");
            OnServerReady(conn);
        }

        void OnServerRemovePlayerMessageInternal(NetworkConnection conn, RemovePlayerMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerRemovePlayerMessageInternal");

            if (conn.identity != null)
            {
                OnServerRemovePlayer(conn, conn.identity);
                conn.identity = null;
            }
        }

        #endregion

        #region Client Internal Message Handlers

        void RegisterClientMessages(NetworkConnection connection)
        {
            connection.RegisterHandler<NetworkConnectionToServer, NotReadyMessage>(OnClientNotReadyMessageInternal);
            connection.RegisterHandler<NetworkConnectionToServer, SceneMessage>(OnClientSceneInternal, false);
        }

        // called after successful authentication
        void OnClientAuthenticated(NetworkConnectionToServer conn)
        {
            RegisterClientMessages(conn);

            if (LogFilter.Debug) Debug.Log("NetworkManager.OnClientAuthenticated");

            // will wait for scene id to come from the server.
            clientLoadedScene = true;
            client.Connection = conn;
        }

        void OnClientNotReadyMessageInternal(NetworkConnection conn, NotReadyMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnClientNotReadyMessageInternal");

            client.ready = false;
            OnClientNotReady(conn);

            // NOTE: clientReadyConnection is not set here! don't want OnClientConnect to be invoked again after scene changes.
        }

        void OnClientSceneInternal(NetworkConnection conn, SceneMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnClientSceneInternal");

            if (client.IsConnected && !server.active)
            {
                ClientChangeScene(msg.sceneName, msg.sceneOperation, msg.customHandling);
            }
        }

        #endregion

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a new client connects.
        /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public virtual void OnServerConnect(NetworkConnection conn) { }


        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public virtual void OnServerReady(NetworkConnection conn)
        {
            if (conn.identity == null)
            {
                // this is now allowed (was not for a while)
                if (LogFilter.Debug) Debug.Log("Ready with no player object");
            }
            server.SetClientReady(conn);
        }


        /// <summary>
        /// Called on the server when a client removes a player.
        /// <para>The default implementation of this function destroys the corresponding player object.</para>
        /// </summary>
        /// <param name="conn">The connection to remove the player from.</param>
        /// <param name="player">The player identity to remove.</param>
        public virtual void OnServerRemovePlayer(NetworkConnection conn, NetworkIdentity player)
        {
            if (player.gameObject != null)
            {
                server.Destroy(player.gameObject);
            }
        }

        /// <summary>
        /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        public virtual void OnServerChangeScene(string newSceneName) { }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        public virtual void OnServerSceneChanged(string sceneName) { }

        #endregion

        #region Client System Callbacks

        /// <summary>
        /// Called on clients when a servers tells the client it is no longer ready.
        /// <para>This is commonly used when switching scenes.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        public virtual void OnClientNotReady(NetworkConnection conn) { }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        public virtual void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        /// <param name="conn">The network connection that the scene change message arrived on.</param>
        public virtual void OnClientSceneChanged(NetworkConnectionToServer conn)
        {
            // always become ready.
            if (!client.ready)
                client.Ready(conn);
        }

        #endregion

    }
}
