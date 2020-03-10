using System;
using System.Collections.Generic;
using System.Linq;
using Mirror.Tcp;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirror
{
    /// <summary>
    /// Enumeration of methods of where to spawn player objects in multiplayer games.
    /// </summary>
    public enum PlayerSpawnMethod { Random, RoundRobin }

    /// <summary>
    /// Enumeration of methods of current Network Manager state at runtime.
    /// </summary>
    public enum NetworkManagerMode { Offline, ServerOnly, ClientOnly, Host }

    [AddComponentMenu("Network/NetworkManager")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkManager.html")]
    [RequireComponent(typeof(NetworkServer))]
    [RequireComponent(typeof(NetworkClient))]
    public class NetworkManager : MonoBehaviour
    {
        /// <summary>
        /// A flag to control whether the NetworkManager object is destroyed when the scene changes.
        /// <para>This should be set if your game has a single NetworkManager that exists for the lifetime of the process. If there is a NetworkManager in each scene, then this should not be set.</para>
        /// </summary>
        [Header("Configuration")]
        [FormerlySerializedAs("dontDestroyOnLoad")]
        [FormerlySerializedAs("m_DontDestroyOnLoad")]
        [Tooltip("Should the Network Manager object be persisted through scene changes?")]
        public new bool DontDestroyOnLoad = true;

        /// <summary>
        /// Controls whether the program runs when it is in the background.
        /// <para>This is required when multiple instances of a program using networking are running on the same machine, such as when testing using localhost. But this is not recommended when deploying to mobile platforms.</para>
        /// </summary>
        [FormerlySerializedAs("runInBackground")]
        [FormerlySerializedAs("m_RunInBackground")]
        [Tooltip("Should the server or client keep running in the background?")]
        public bool RunInBackground = true;

        /// <summary>
        /// Automatically invoke StartServer()
        /// <para>If the application is a Server Build or run with the -batchMode command line arguement, StartServer is automatically invoked.</para>
        /// </summary>
        [FormerlySerializedAs("startOnHeadless")]
        [Tooltip("Should the server auto-start when the game is started in a headless build?")]
        public bool StartOnHeadless = true;

        /// <summary>
        /// Enables verbose debug messages in the console
        /// </summary>
        [FormerlySerializedAs("showDebugMessages")]
        [FormerlySerializedAs("m_ShowDebugMessages")]
        [Tooltip("This will enable verbose debug messages in the Unity Editor console")]
        public bool ShowDebugMessages;

        /// <summary>
        /// Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.
        /// </summary>
        [FormerlySerializedAs("serverTickRate")]
        [Tooltip("Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.")]
        public int ServerTickRate = 30;

        /// <summary>
        /// The scene to switch to when offline.
        /// <para>Setting this makes the NetworkManager do scene management. This scene will be switched to when a network session is completed - such as a client disconnect, or a server shutdown.</para>
        /// </summary>
        [Header("Scene Management")]
        [Scene]
        [FormerlySerializedAs("offlineScene")]
        [FormerlySerializedAs("m_OfflineScene")]
        [Tooltip("Scene that Mirror will switch to when the client or server is stopped")]
        public string OfflineScene = "";

        /// <summary>
        /// The scene to switch to when online.
        /// <para>Setting this makes the NetworkManager do scene management. This scene will be switched to when a network session is started - such as a client connect, or a server listen.</para>
        /// </summary>
        [Scene]
        [FormerlySerializedAs("onlineScene")]
        [FormerlySerializedAs("m_OnlineScene")]
        [Tooltip("Scene that Mirror will switch to when the server is started. Clients will recieve a Scene Message to load the server's current scene when they connect.")]
        public string OnlineScene = "";

        [FormerlySerializedAs("server")]
        public NetworkServer Server;
        [FormerlySerializedAs("client")]
        public NetworkClient Client;

        // transport layer
        [Header("Network Info")]
        [FormerlySerializedAs("transport")]
        [Tooltip("Transport component attached to this object that server and client will use to connect")]
        [SerializeField]
        protected Transport Transport;

        /// <summary>
        /// The maximum number of concurrent network connections to support.
        /// <para>This effects the memory usage of the network layer.</para>
        /// </summary>
        [FormerlySerializedAs("maxConnections")]
        [FormerlySerializedAs("m_MaxConnections")]
        [Tooltip("Maximum number of concurrent connections.")]
        public int MaxConnections = 4;

        [Header("Authentication")]
        [FormerlySerializedAs("authenticator")]
        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator Authenticator;

        /// <summary>
        /// The default prefab to be used to create player objects on the server.
        /// <para>Player objects are created in the default handler for AddPlayer() on the server. Implementing OnServerAddPlayer overrides this behaviour.</para>
        /// </summary>
        [Header("Player Object")]
        [FormerlySerializedAs("playerPrefab")]
        [FormerlySerializedAs("m_PlayerPrefab")]
        [Tooltip("Prefab of the player object. Prefab must have a Network Identity component. May be an empty game object or a full avatar.")]
        public GameObject PlayerPrefab;

        /// <summary>
        /// A flag to control whether or not player objects are automatically created on connect, and on scene change.
        /// </summary>
        [FormerlySerializedAs("autoCreatePlayer")]
        [FormerlySerializedAs("m_AutoCreatePlayer")]
        [Tooltip("Should Mirror automatically spawn the player after scene change?")]
        public bool AutoCreatePlayer = true;

        /// <summary>
        /// The current method of spawning players used by the NetworkManager.
        /// </summary>
        [FormerlySerializedAs("playerSpawnMethod")]
        [FormerlySerializedAs("m_PlayerSpawnMethod")]
        [Tooltip("Round Robin or Random order of Start Position selection")]
        public PlayerSpawnMethod PlayerSpawnMethod;

        /// <summary>
        /// List of prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.</para>
        /// </summary>
        [FormerlySerializedAs("spawnPrefabs")]
        [FormerlySerializedAs("m_SpawnPrefabs")]
        public List<GameObject> SpawnPrefabs = new List<GameObject>();

        /// <summary>
        /// Number of active player objects across all connections on the server.
        /// <para>This is only valid on the host / server.</para>
        /// </summary>
        public int NumberOfActivePlayers => Server.connections.Count(kv => kv.Value.identity != null);

        /// <summary>
        /// True if the server or client is started and running
        /// <para>This is set True in StartServer / StartClient, and set False in StopServer / StopClient</para>
        /// </summary>
        [NonSerialized]
        public bool IsNetworkActive;

        private NetworkConnection clientReadyConnection;

        /// <summary>
        /// This is true if the client loaded a new scene when connecting to the server.
        /// <para>This is set before OnClientConnect is called, so it can be checked there to perform different logic if a scene load occurred.</para>
        /// </summary>
        [NonSerialized]
        private bool clientLoadedScene;

        // Deprecated 03/27/2019
        /// <summary>
        /// headless mode detection
        /// </summary>
        public static bool IsHeadless => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

        // helper enum to know if we started the networkmanager as server/client/host.
        // -> this is necessary because when StartHost changes server scene to
        //    online scene, FinishLoadScene is called and the host client isn't
        //    connected yet (no need to connect it before server was fully set up).
        //    in other words, we need this to know which mode we are running in
        //    during FinishLoadScene.
        public NetworkManagerMode Mode { get; private set; }

        #region Unity Callbacks

        /// <summary>
        /// virtual so that inheriting classes' OnValidate() can call base.OnValidate() too
        /// </summary>
        public virtual void OnValidate()
        {
            // add transport if there is none yet. makes upgrading easier.
            if (Transport == null)
            {
                // was a transport added yet? if not, add one
                Transport = GetComponent<Transport>();
                if (Transport == null)
                {
                    Transport = gameObject.AddComponent<TcpTransport>();
                    Debug.Log("NetworkManager: added default Transport because there was none yet.");
                }
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added default Transport");
#endif
            }

            // add NetworkServer if there is none yet. makes upgrading easier.
            if (GetComponent<NetworkServer>() == null)
            {
                Server = gameObject.AddComponent<NetworkServer>();
                Debug.Log("NetworkManager: added NetworkServer because there was none yet.");
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added NetworkServer");
#endif
            }

            // add NetworkClient if there is none yet. makes upgrading easier.
            if (GetComponent<NetworkClient>() == null)
            {
                Client = gameObject.AddComponent<NetworkClient>();
                Debug.Log("NetworkManager: added NetworkClient because there was none yet.");
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added NetworkClient");
#endif
            }

            MaxConnections = Mathf.Max(MaxConnections, 0); // always >= 0

            if (PlayerPrefab == null || PlayerPrefab.GetComponent<NetworkIdentity>() != null)
                return;

            Debug.LogError("NetworkManager - playerPrefab must have a NetworkIdentity.");
            PlayerPrefab = null;
        }

        /// <summary>
        /// virtual so that inheriting classes' Awake() can call base.Awake() too
        /// </summary>
        public virtual void Awake()
        {
            // Set the networkSceneName to prevent a scene reload
            // if client connection to server fails.
            NetworkSceneName = OfflineScene;

            Initialize();

            // setup OnSceneLoaded callback
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// virtual so that inheriting classes' Start() can call base.Start() too
        /// </summary>
        public virtual void Start()
        {
            // headless mode? then start the server
            // can't do this in Awake because Awake is for initialization.
            // some transports might not be ready until Start.
            //
            // (tick rate is applied in StartServer!)
            if (IsHeadless && StartOnHeadless)
                StartServer();
        }

        // NetworkIdentity.UNetStaticUpdate is called from UnityEngine while LLAPI network is active.
        // If we want TCP then we need to call it manually. Probably best from NetworkManager, although this means that we can't use NetworkServer/NetworkClient without a NetworkManager invoking Update anymore.
        /// <summary>
        /// virtual so that inheriting classes' LateUpdate() can call base.LateUpdate() too
        /// </summary>
        public virtual void LateUpdate()
        {
            // call it while the NetworkManager exists.
            // -> we don't only call while Client/Server.Connected, because then we would stop if disconnected and the
            //    NetworkClient wouldn't receive the last Disconnect event, result in all kinds of issues
            Server.Update();
            Client.Update();
            UpdateScene();
        }

        #endregion

        #region Start & Stop

        // keep the online scene change check in a separate function
        private bool IsServerOnlineSceneChangeNeeded()
        {
            // Only change scene if the requested online scene is not blank, and is not already loaded
            string loadedSceneName = SceneManager.GetActiveScene().name;

            return !string.IsNullOrEmpty(OnlineScene) && OnlineScene != loadedSceneName && OnlineScene != OfflineScene;
        }

        // full server setup code, without spawning objects yet
        private void SetupServer()
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager SetupServer");

            Initialize();

            if (RunInBackground)
                Application.runInBackground = true;

            if (Authenticator != null)
            {
                Authenticator.OnStartServer();
                Authenticator.OnServerAuthenticated += OnServerAuthenticated;
            }

            ConfigureServerFrameRate();

            // start listening to network connections
            Server.Listen(MaxConnections);

            // call OnStartServer AFTER Listen, so that NetworkServer.active is
            // true and we can call NetworkServer.Spawn in OnStartServer
            // overrides.
            // (useful for loading & spawning stuff from database etc.)
            //
            // note: there is no risk of someone connecting after Listen() and
            //       before OnStartServer() because this all runs in one thread
            //       and we don't start processing connects until Update.
            OnStartServer();

            // this must be after Listen(), since that registers the default message handlers
            RegisterServerMessages();

            IsNetworkActive = true;
        }

        /// <summary>
        /// This starts a new server.
        /// <para>This uses the networkPort property as the listen port.</para>
        /// </summary>
        /// <returns></returns>
        public void StartServer()
        {
            Mode = NetworkManagerMode.ServerOnly;

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

            // scene change needed? then change scene and spawn afterwards.
            if (IsServerOnlineSceneChangeNeeded())
                ServerChangeScene(OnlineScene);
            // otherwise spawn directly
            else
                Server.SpawnObjects();
        }

        /// <summary>
        /// This starts a network client. It uses the networkAddress and networkPort properties as the address to connect to.
        /// <para>This makes the newly created client connect to the server immediately.</para>
        /// </summary>
        public void StartClient(string serverIp)
        {
            Mode = NetworkManagerMode.ClientOnly;

            Initialize();

            if (Authenticator != null)
            {
                Authenticator.OnStartClient();
                Authenticator.OnClientAuthenticated += OnClientAuthenticated;
            }

            if (RunInBackground)
                Application.runInBackground = true;

            IsNetworkActive = true;

            RegisterClientMessages();

            if (string.IsNullOrEmpty(serverIp))
            {
                Debug.LogError("serverIp shouldn't be empty");
                return;
            }

            if (LogFilter.Debug)
                Debug.Log("NetworkManager StartClient address:" + serverIp);

            _ = Client.ConnectAsync(serverIp);

            OnStartClient();
        }

        /// <summary>
        /// This starts a network client. It uses the networkAddress and networkPort properties as the address to connect to.
        /// <para>This makes the newly created client connect to the server immediately.</para>
        /// </summary>
        /// <param name="uri">location of the server to connect to</param>
        public void StartClient(Uri uri)
        {
            Mode = NetworkManagerMode.ClientOnly;

            Initialize();

            if (Authenticator != null)
            {
                Authenticator.OnStartClient();
                Authenticator.OnClientAuthenticated += OnClientAuthenticated;
            }

            if (RunInBackground)
                Application.runInBackground = true;

            IsNetworkActive = true;

            RegisterClientMessages();

            if (LogFilter.Debug)
                Debug.Log("NetworkManager StartClient address:" + uri);

            _ = Client.ConnectAsync(uri);

            OnStartClient();
        }

        /// <summary>
        /// This starts a network "host" - a server and client in the same application.
        /// <para>The client returned from StartHost() is a special "local" client that communicates to the in-process server using a message queue instead of the real network. But in almost all other cases, it can be treated as a normal client.</para>
        /// </summary>
        public void StartHost()
        {
            Mode = NetworkManagerMode.Host;

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
            OnStartHost();

            // scene change needed? then change scene and spawn afterwards.
            // => BEFORE host client connects. if client auth succeeds then the
            //    server tells it to load 'onlineScene'. we can't do that if
            //    server is still in 'offlineScene'. so load on server first.
            if (IsServerOnlineSceneChangeNeeded())
            {
                // call FinishStartHost after changing scene.
                finishStartHostPending = true;
                ServerChangeScene(OnlineScene);
            }
            // otherwise call FinishStartHost directly
            else
            {
                FinishStartHost();
            }
        }

        // This may be set true in StartHost and is evaluated in FinishStartHost
        private bool finishStartHostPending;

        // FinishStartHost is guaranteed to be called after the host server was
        // fully started and all the asynchronous StartHost magic is finished
        // (= scene loading), or immediately if there was no asynchronous magic.
        //
        // note: we don't really need FinishStartClient/FinishStartServer. the
        //       host version is enough.
        private void FinishStartHost()
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
            Client.ConnectHost(Server);

            // server scene was loaded. now spawn all the objects
            Server.SpawnObjects();

            // connect client and call OnStartClient AFTER server scene was
            // loaded and all objects were spawned.
            // DO NOT do this earlier. it would cause race conditions where a
            // client will do things before the server is even fully started.
            Debug.Log("StartHostClient called");
            StartHostClient();
        }

        private void StartHostClient()
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager ConnectLocalClient");

            if (Authenticator != null)
            {
                Authenticator.OnStartClient();
                Authenticator.OnClientAuthenticated += OnClientAuthenticated;
            }

            Server.ActivateHostScene();
            RegisterClientMessages();

            // ConnectLocalServer needs to be called AFTER RegisterClientMessages
            // (https://github.com/vis2k/Mirror/pull/1249/)
            Client.ConnectLocalServer(Server);

            OnStartClient();
        }

        /// <summary>
        /// This stops both the client and the server that the manager is using.
        /// </summary>
        public void StopHost()
        {
            OnStopHost();
            StopServer();
            StopClient();
        }

        /// <summary>
        /// Stops the server that the manager is using.
        /// </summary>
        public void StopServer()
        {
            if (!Server.active)
                return;

            if (Authenticator != null)
                Authenticator.OnServerAuthenticated -= OnServerAuthenticated;

            OnStopServer();

            if (LogFilter.Debug)
                Debug.Log("NetworkManager StopServer");

            IsNetworkActive = false;
            Server.Shutdown();

            // set offline mode BEFORE changing scene so that FinishStartScene
            // doesn't think we need initialize anything.
            Mode = NetworkManagerMode.Offline;

            if (!string.IsNullOrEmpty(OfflineScene))
                ServerChangeScene(OfflineScene);

            CleanupNetworkIdentities();

            StartPositionIndex = 0;
        }

        /// <summary>
        /// Stops the client that the manager is using.
        /// </summary>
        public void StopClient()
        {
            if (Authenticator != null)
                Authenticator.OnClientAuthenticated -= OnClientAuthenticated;

            OnStopClient();

            if (LogFilter.Debug)
                Debug.Log("NetworkManager StopClient");

            IsNetworkActive = false;

            // shutdown client
            Client.Disconnect();
            Client.Shutdown();

            // set offline mode BEFORE changing scene so that FinishStartScene
            // doesn't think we need initialize anything.
            Mode = NetworkManagerMode.Offline;

            // If this is the host player, StopServer will already be changing scenes.
            // Check loadingSceneAsync to ensure we don't double-invoke the scene change.
            if (!string.IsNullOrEmpty(OfflineScene) && SceneManager.GetActiveScene().name != OfflineScene && LoadingSceneAsync == null)
                ClientChangeScene(OfflineScene, SceneOperation.Normal);

            CleanupNetworkIdentities();
        }

        /// <summary>
        /// called when quitting the application by closing the window / pressing stop in the editor
        /// <para>virtual so that inheriting classes' OnApplicationQuit() can call base.OnApplicationQuit() too</para>
        /// </summary>
        public virtual void OnApplicationQuit()
        {
            // stop client first
            // (we want to send the quit packet to the server instead of waiting
            //  for a timeout)
            if (Client.isConnected)
            {
                StopClient();
                print("OnApplicationQuit: stopped client");
            }

            // stop server after stopping client (for proper host mode stopping)
            if (Server.active)
            {
                StopServer();
                print("OnApplicationQuit: stopped server");
            }
        }

        /// <summary>
        /// Set the frame rate for a headless server.
        /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
        /// </summary>
        protected virtual void ConfigureServerFrameRate()
        {
            // set a fixed tick rate instead of updating as often as possible
            // * if not in Editor (it doesn't work in the Editor)
            // * if not in Host mode
#if !UNITY_EDITOR
            if (!Client.active && IsHeadless)
            {
                Application.targetFrameRate = ServerTickRate;
                Debug.Log("Server Tick Rate set to: " + Application.targetFrameRate + " Hz.");
            }
#endif
        }

        private void Initialize()
        {
            LogFilter.Debug = ShowDebugMessages;

            if (DontDestroyOnLoad)
            {
                // using FindObjectsOfType here is not a big deal, since it is not a hot path
                // it would occur only on a scene change AND when a new NetworkManager awakens
                NetworkManager[] managers = FindObjectsOfType<NetworkManager>();

                if (managers.Length > 1)
                {
                    foreach (NetworkManager manager in managers)
                    {
                        if (manager != this && manager.DontDestroyOnLoad)
                            Destroy(manager.gameObject);
                    }
                }

                DontDestroyOnLoad(gameObject);
            }

            Transport.activeTransport = Transport;
        }

        void RegisterServerMessages()
        {
            Server.RegisterHandler<ConnectMessage>(OnServerConnectInternal, false);
            Server.RegisterHandler<DisconnectMessage>(OnServerDisconnectInternal, false);
            Server.RegisterHandler<ReadyMessage>(OnServerReadyMessageInternal);
            Server.RegisterHandler<AddPlayerMessage>(OnServerAddPlayerInternal);
            Server.RegisterHandler<RemovePlayerMessage>(OnServerRemovePlayerMessageInternal);
            Server.RegisterHandler<ErrorMessage>(OnServerErrorInternal, false);
        }

        void RegisterClientMessages()
        {
            Client.RegisterHandler<ConnectMessage>(OnClientConnectInternal, false);
            Client.RegisterHandler<DisconnectMessage>(OnClientDisconnectInternal, false);
            Client.RegisterHandler<NotReadyMessage>(OnClientNotReadyMessageInternal);
            Client.RegisterHandler<ErrorMessage>(OnClientErrorInternal, false);
            Client.RegisterHandler<SceneMessage>(OnClientSceneInternal, false);

            if (PlayerPrefab != null)
            {
                ClientScene.RegisterPrefab(PlayerPrefab);
            }

            for (int i = 0; i < SpawnPrefabs.Count; i++)
            {
                GameObject prefab = SpawnPrefabs[i];
                if (prefab != null)
                {
                    ClientScene.RegisterPrefab(prefab);
                }
            }
        }

        private void CleanupNetworkIdentities()
        {
            foreach (NetworkIdentity identity in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
            {
                identity.MarkForReset();
            }
        }

        /// <summary>
        /// virtual so that inheriting classes' OnDestroy() can call base.OnDestroy() too
        /// </summary>
        public virtual void OnDestroy()
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager destroyed");
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
        [FormerlySerializedAs("networkSceneName")]
        public string NetworkSceneName = "";

        [FormerlySerializedAs("loadingSceneAsync")]
        public AsyncOperation LoadingSceneAsync;

        /// <summary>
        /// This causes the server to switch scenes and sets the networkSceneName.
        /// <para>Clients that connect to this server will automatically switch to this scene. This is called autmatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
        /// </summary>
        /// <param name="newSceneName"></param>
        protected virtual void ServerChangeScene(string newSceneName)
        {
            if (string.IsNullOrEmpty(newSceneName))
            {
                Debug.LogError("ServerChangeScene empty scene name");
                return;
            }

            if (LogFilter.Debug)
                Debug.Log("ServerChangeScene " + newSceneName);

            Server.SetAllClientsNotReady();
            NetworkSceneName = newSceneName;

            // Let server prepare for scene change
            OnServerChangeScene(newSceneName);

            // Suspend the server's transport while changing scenes
            // It will be re-enabled in FinishScene.
            Transport.activeTransport.enabled = false;

            ClientScene.server = Server;
            ClientScene.client = Client;

            LoadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);

            // notify all clients about the new scene
            Server.SendToAll(new SceneMessage { sceneName = newSceneName });

            StartPositionIndex = 0;
            StartPositions.Clear();
        }

        // This is only set in ClientChangeScene below...never on server.
        // We need to check this in OnClientSceneChanged called from FinishLoadSceneClientOnly
        // to prevent AddPlayer message after loading/unloading additive scenes
        private SceneOperation clientSceneOperation = SceneOperation.Normal;

        private void ClientChangeScene(string newSceneName, SceneOperation sceneOperation = SceneOperation.Normal, bool customHandling = false)
        {
            if (string.IsNullOrEmpty(newSceneName))
            {
                Debug.LogError("ClientChangeScene empty scene name");
                return;
            }

            if (LogFilter.Debug)
                Debug.Log("ClientChangeScene newSceneName:" + newSceneName + " networkSceneName:" + NetworkSceneName);

            // vis2k: pause message handling while loading scene. otherwise we will process messages and then lose all
            // the state as soon as the load is finishing, causing all kinds of bugs because of missing state.
            // (client may be null after StopClient etc.)
            if (LogFilter.Debug)
                Debug.Log("ClientChangeScene: pausing handlers while scene is loading to avoid data loss after scene was loaded.");

            Transport.activeTransport.enabled = false;

            ClientScene.server = Server;
            ClientScene.client = Client;

            // Let client prepare for scene change
            OnClientChangeScene(newSceneName, sceneOperation, customHandling);

            // scene handling will happen in overrides of OnClientChangeScene and/or OnClientSceneChanged
            if (customHandling)
            {
                FinishLoadScene();
                return;
            }

            // cache sceneOperation so we know what was done in OnClientSceneChanged called from FinishLoadSceneClientOnly
            clientSceneOperation = sceneOperation;

            switch (sceneOperation)
            {
                case SceneOperation.Normal:
                    LoadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
                    break;
                case SceneOperation.LoadAdditive:
                    if (!SceneManager.GetSceneByName(newSceneName).IsValid())
                        LoadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
                    else
                        Debug.LogWarningFormat("Scene {0} is already loaded", newSceneName);
                    break;
                case SceneOperation.UnloadAdditive:
                    if (SceneManager.GetSceneByName(newSceneName).IsValid())
                    {
                        if (SceneManager.GetSceneByName(newSceneName) != null)
                            LoadingSceneAsync = SceneManager.UnloadSceneAsync(newSceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                    }
                    else
                        Debug.LogWarning("Cannot unload the active scene with UnloadAdditive operation");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sceneOperation), sceneOperation, null);
            }

            // don't change the client's current networkSceneName when loading additive scene content
            if (sceneOperation == SceneOperation.Normal)
                NetworkSceneName = newSceneName;
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
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Additive)
                return;

            if (Server.active)
            {
                // TODO only respawn the server objects from that scene later!
                Server.SpawnObjects();
                Debug.Log("Respawned Server objects after additive scene load: " + scene.name);
            }

            if (Client.active)
            {
                ClientScene.PrepareToSpawnSceneObjects();
                Debug.Log("Rebuild Client spawnableObjects after additive scene load: " + scene.name);
            }
        }

        private void UpdateScene()
        {
            if (LoadingSceneAsync == null || !LoadingSceneAsync.isDone)
                return;

            if (LogFilter.Debug)
                Debug.Log("ClientChangeScene done readyCon:" + clientReadyConnection);

            FinishLoadScene();
            LoadingSceneAsync.allowSceneActivation = true;
            LoadingSceneAsync = null;
        }

        private void FinishLoadScene()
        {
            // NOTE: this cannot use NetworkClient.allClients[0] - that client may be for a completely different purpose.

            // process queued messages that we received while loading the scene
            if (LogFilter.Debug) Debug.Log("FinishLoadScene: resuming handlers after scene was loading.");
            Transport.activeTransport.enabled = true;

            switch (Mode)
            {
                // host mode?
                case NetworkManagerMode.Host:
                    FinishLoadSceneHost();
                    break;
                case NetworkManagerMode.ServerOnly:
                    FinishLoadSceneServerOnly();
                    break;
                // server-only mode?
                case NetworkManagerMode.ClientOnly:
                    FinishLoadSceneClientOnly();
                    break;
                // client-only mode?
                case NetworkManagerMode.Offline:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // otherwise we called it after stopping when loading offline scene.
            // do nothing then.
        }

        // finish load scene part for host mode. makes code easier and is
        // necessary for FinishStartHost later.
        // (the 3 things have to happen in that exact order)
        private void FinishLoadSceneHost()
        {
            // debug message is very important. if we ever break anything then
            // it's very obvious to notice.
            Debug.Log("Finished loading scene in host mode.");

            if (clientReadyConnection != null)
            {
                OnClientConnect(clientReadyConnection);
                clientLoadedScene = true;
                clientReadyConnection = null;
            }

            // do we need to finish a StartHost() call?
            // then call FinishStartHost and let it take care of spawning etc.
            if (finishStartHostPending)
            {
                finishStartHostPending = false;
                FinishStartHost();

                // call OnServerSceneChanged
                OnServerSceneChanged(NetworkSceneName);

                if (Client.isConnected)
                {
                    RegisterClientMessages();

                    // DO NOT call OnClientSceneChanged here.
                    // the scene change happened because StartHost loaded the
                    // server's online scene. it has nothing to do with the client.
                    // this was not meant as a client scene load, so don't call it.
                    //
                    // otherwise AddPlayer would be called twice:
                    // -> once for client OnConnected
                    // -> once in OnClientSceneChanged
                }
            }
            // otherwise we just changed a scene in host mode
            else
            {
                // spawn server objects
                Server.SpawnObjects();

                // call OnServerSceneChanged
                OnServerSceneChanged(NetworkSceneName);

                if (Client.isConnected)
                {
                    RegisterClientMessages();

                    // let client know that we changed scene
                    OnClientSceneChanged(Client.connection);
                }
            }
        }

        // finish load scene part for client-only. makes code easier and is
        // necessary for FinishStartClient later.
        private void FinishLoadSceneClientOnly()
        {
            // debug message is very important. if we ever break anything then
            // it's very obvious to notice.
            Debug.Log("Finished loading scene in client-only mode.");

            if (clientReadyConnection != null)
            {
                OnClientConnect(clientReadyConnection);
                clientLoadedScene = true;
                clientReadyConnection = null;
            }

            if (Client.isConnected)
            {
                RegisterClientMessages();
                OnClientSceneChanged(Client.connection);
            }
        }

        // finish load scene part for server-only. . makes code easier and is
        // necessary for FinishStartServer later.
        private void FinishLoadSceneServerOnly()
        {
            // debug message is very important. if we ever break anything then
            // it's very obvious to notice.
            Debug.Log("Finished loading scene in server-only mode.");

            Server.SpawnObjects();
            OnServerSceneChanged(NetworkSceneName);
        }

        #endregion

        #region Start Positions

        [FormerlySerializedAs("startPositionIndex")]
        public int StartPositionIndex;

        /// <summary>
        /// List of transforms populted by NetworkStartPosition components found in the scene.
        /// </summary>
        [FormerlySerializedAs("startPositions")]
        public List<Transform> StartPositions = new List<Transform>();

        /// <summary>
        /// Registers the transform of a game object as a player spawn location.
        /// <para>This is done automatically by NetworkStartPosition components, but can be done manually from user script code.</para>
        /// </summary>
        /// <param name="start">Transform to register.</param>
        public void RegisterStartPosition(Transform start)
        {
            if (LogFilter.Debug)
                Debug.Log("RegisterStartPosition: (" + start.gameObject.name + ") " + start.position);

            StartPositions.Add(start);

            // reorder the list so that round-robin spawning uses the start positions
            // in hierarchy order.  This assumes all objects with NetworkStartPosition
            // component are siblings, either in the scene root or together as children
            // under a single parent in the scene.
            StartPositions = StartPositions.OrderBy(transform => transform.GetSiblingIndex()).ToList();
        }

        /// <summary>
        /// Unregisters the transform of a game object as a player spawn location.
        /// <para>This is done automatically by the <see cref="NetworkStartPosition">NetworkStartPosition</see> component, but can be done manually from user code.</para>
        /// </summary>
        /// <param name="start">Transform to unregister.</param>
        public void UnRegisterStartPosition(Transform start)
        {
            if (LogFilter.Debug)
                Debug.Log("UnRegisterStartPosition: (" + start.gameObject.name + ") " + start.position);

            StartPositions.Remove(start);
        }

        #endregion

        #region Server Internal Message Handlers

        private void OnServerConnectInternal(NetworkConnectionToClient conn, ConnectMessage connectMsg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnServerConnectInternal");

            if (Authenticator != null)
            {
                // we have an authenticator - let it handle authentication
                Authenticator.OnServerAuthenticateInternal(conn);
            }
            else
            {
                // authenticate immediately
                OnServerAuthenticated(conn);
            }
        }

        // called after successful authentication
        private void OnServerAuthenticated(NetworkConnectionToClient conn)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnServerAuthenticated");

            // set connection to authenticated
            conn.isAuthenticated = true;

            // proceed with the login handshake by calling OnServerConnect
            if (NetworkSceneName != "" && NetworkSceneName != OfflineScene)
            {
                var msg = new SceneMessage() { sceneName = NetworkSceneName };
                conn.Send(msg);
            }

            OnServerConnect(conn);
        }

        private void OnServerDisconnectInternal(NetworkConnection conn, DisconnectMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnServerDisconnectInternal");

            OnServerDisconnect(conn);
        }

        private void OnServerReadyMessageInternal(NetworkConnection conn, ReadyMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnServerReadyMessageInternal");

            OnServerReady(conn);
        }

        private void OnServerAddPlayerInternal(NetworkConnection conn, AddPlayerMessage extraMessage)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnServerAddPlayer");

            if (AutoCreatePlayer && PlayerPrefab == null)
            {
                Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
                return;
            }

            if (AutoCreatePlayer && PlayerPrefab.GetComponent<NetworkIdentity>() == null)
            {
                Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
                return;
            }

            if (conn.identity != null)
            {
                Debug.LogError("There is already a player for this connection.");
                return;
            }

            OnServerAddPlayer(conn);
        }

        private void OnServerRemovePlayerMessageInternal(NetworkConnection conn, RemovePlayerMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnServerRemovePlayerMessageInternal");

            if (conn.identity == null)
                return;

            OnServerRemovePlayer(conn, conn.identity);
            conn.identity = null;
        }

        private void OnServerErrorInternal(NetworkConnection conn, ErrorMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnServerErrorInternal");

            OnServerError(conn, msg.value);
        }

        #endregion

        #region Client Internal Message Handlers

        private void OnClientConnectInternal(NetworkConnectionToServer conn, ConnectMessage message)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnClientConnectInternal");

            if (Authenticator != null)
            {
                // we have an authenticator - let it handle authentication
                Authenticator.OnClientAuthenticateInternal(conn);
            }
            else
            {
                // authenticate immediately
                OnClientAuthenticated(conn);
            }
        }

        // called after successful authentication
        private void OnClientAuthenticated(NetworkConnection conn)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnClientAuthenticated");

            // set connection to authenticated
            conn.isAuthenticated = true;

            // proceed with the login handshake by calling OnClientConnect
            string loadedSceneName = SceneManager.GetActiveScene().name;

            if (string.IsNullOrEmpty(OnlineScene) || OnlineScene == OfflineScene || loadedSceneName == OnlineScene)
            {
                clientLoadedScene = false;
                OnClientConnect(conn);
            }
            else
            {
                // will wait for scene id to come from the server.
                clientLoadedScene = true;
                clientReadyConnection = conn;
            }
        }

        private void OnClientDisconnectInternal(NetworkConnection conn, DisconnectMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnClientDisconnectInternal");

            OnClientDisconnect(conn);
        }

        private void OnClientNotReadyMessageInternal(NetworkConnection conn, NotReadyMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnClientNotReadyMessageInternal");

            ClientScene.ready = false;
            OnClientNotReady(conn);

            // NOTE: clientReadyConnection is not set here! don't want OnClientConnect to be invoked again after scene changes.
        }

        private void OnClientErrorInternal(NetworkConnection conn, ErrorMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager:OnClientErrorInternal");

            OnClientError(conn, msg.value);
        }

        private void OnClientSceneInternal(NetworkConnection conn, SceneMessage msg)
        {
            if (LogFilter.Debug)
                Debug.Log("NetworkManager.OnClientSceneInternal");

            if (Client.isConnected && !Server.active)
                ClientChangeScene(msg.sceneName, msg.sceneOperation, msg.customHandling);
        }

        #endregion

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a new client connects.
        /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        protected virtual void OnServerConnect(NetworkConnection conn) { }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        protected virtual void OnServerDisconnect(NetworkConnection conn)
        {
            Server.DestroyPlayerForConnection(conn);

            if (LogFilter.Debug)
                Debug.Log("OnServerDisconnect: Client disconnected.");
        }

        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        protected virtual void OnServerReady(NetworkConnection conn)
        {
            if (conn.identity == null)
            {
                // this is now allowed (was not for a while)
                if (LogFilter.Debug)
                    Debug.Log("Ready with no player object");
            }

            Server.SetClientReady(conn);
        }

        /// <summary>
        /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        protected virtual void OnServerAddPlayer(NetworkConnection conn)
        {
            Transform startPos = GetStartPosition();

            GameObject player = startPos != null
                ? Instantiate(PlayerPrefab, startPos.position, startPos.rotation)
                : Instantiate(PlayerPrefab);

            Server.AddPlayerForConnection(conn, player);
        }

        /// <summary>
        /// This finds a spawn position based on NetworkStartPosition objects in the scene.
        /// <para>This is used by the default implementation of OnServerAddPlayer.</para>
        /// </summary>
        /// <returns>Returns the transform to spawn a player at, or null.</returns>
        public Transform GetStartPosition()
        {
            // first remove any dead transforms
            StartPositions.RemoveAll(t => t == null);

            if (StartPositions.Count == 0)
                return null;

            if (PlayerSpawnMethod == PlayerSpawnMethod.Random)
            {
                return StartPositions[UnityEngine.Random.Range(0, StartPositions.Count)];
            }

            Transform startPosition = StartPositions[StartPositionIndex];
            StartPositionIndex = (StartPositionIndex + 1) % StartPositions.Count;

            return startPosition;
        }

        /// <summary>
        /// Called on the server when a client removes a player.
        /// <para>The default implementation of this function destroys the corresponding player object.</para>
        /// </summary>
        /// <param name="conn">The connection to remove the player from.</param>
        /// <param name="player">The player identity to remove.</param>
        protected virtual void OnServerRemovePlayer(NetworkConnection conn, NetworkIdentity player)
        {
            if (player.gameObject != null)
                Server.Destroy(player.gameObject);
        }

        /// <summary>
        /// Called on the server when a network error occurs for a client connection.
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        /// <param name="errorCode">Error code.</param>
        protected virtual void OnServerError(NetworkConnection conn, int errorCode) { }

        /// <summary>
        /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        protected virtual void OnServerChangeScene(string newSceneName) { }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        protected virtual void OnServerSceneChanged(string sceneName) { }

        #endregion

        #region Client System Callbacks

        /// <summary>
        /// Called on the client when connected to a server.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        protected virtual void OnClientConnect(NetworkConnection conn)
        {
            // OnClientConnect by default calls AddPlayer but it should not do
            // that when we have online/offline scenes. so we need the
            // clientLoadedScene flag to prevent it.
            if (clientLoadedScene)
                return;

            ClientScene.server = Server;
            ClientScene.client = Client;

            // Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
            if (!ClientScene.ready)
                ClientScene.Ready(conn);

            if (AutoCreatePlayer)
                ClientScene.AddPlayer();
        }

        /// <summary>
        /// Called on clients when disconnected from a server.
        /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        protected virtual void OnClientDisconnect(NetworkConnection conn)
        {
            StopClient();
        }

        /// <summary>
        /// Called on clients when a network error occurs.
        /// </summary>
        /// <param name="conn">Connection to a server.</param>
        /// <param name="errorCode">Error code.</param>
        protected virtual void OnClientError(NetworkConnection conn, int errorCode) { }

        /// <summary>
        /// Called on clients when a servers tells the client it is no longer ready.
        /// <para>This is commonly used when switching scenes.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        protected virtual void OnClientNotReady(NetworkConnection conn) { }

        // Deprecated 09/17/2019
        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        protected virtual void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        /// <param name="conn">The network connection that the scene change message arrived on.</param>
        protected virtual void OnClientSceneChanged(NetworkConnection conn)
        {
            // always become ready.
            if (!ClientScene.ready)
                ClientScene.Ready(conn);

            // Only call AddPlayer for normal scene changes, not additive load/unload
            if (clientSceneOperation == SceneOperation.Normal && AutoCreatePlayer && ClientScene.localPlayer == null)
            {
                // add player if existing one is null
                ClientScene.AddPlayer();
            }
        }

        #endregion

        #region Start & Stop callbacks

        // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
        // their functionality, users would need override all the versions. Instead these callbacks are invoked
        // from all versions, so users only need to implement this one case.

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        protected virtual void OnStartHost() { }

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        protected virtual void OnStartServer() { }

        // Deprecated 03/25/2019
        /// <summary>
        /// This is invoked when the client is started.
        /// </summary>
        protected virtual void OnStartClient() { }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        protected virtual void OnStopServer() { }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        protected virtual void OnStopClient() { }

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        protected virtual void OnStopHost() { }

        #endregion
    }
}
