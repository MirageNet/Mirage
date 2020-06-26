using System;
using System.Threading.Tasks;
using Mirror.AsyncTcp;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Mirror
{

    [AddComponentMenu("Network/NetworkManager")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkManager.html")]
    [RequireComponent(typeof(NetworkServer))]
    [RequireComponent(typeof(NetworkClient))]
    [DisallowMultipleComponent]
    public class NetworkManager : MonoBehaviour, INetworkManager
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkManager));

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
        protected AsyncTransport transport;

        /// <summary>
        /// True if the server or client is started and running
        /// <para>This is set True in StartServer / StartClient, and set False in StopServer / StopClient</para>
        /// </summary>
        public bool IsNetworkActive => server.Active || client.Active;

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
        /// virtual so that inheriting classes' Reset() can call base.Reset() too
        /// </summary>
        public virtual void OnValidate()
        {
            // add transport if there is none yet. makes upgrading easier.
            if (transport == null)
            {
                // was a transport added yet? if not, add one
                transport = GetComponent<AsyncTransport>();
                if (transport == null)
                {
                    transport = gameObject.AddComponent<AsyncTcpTransport>();
                    logger.Log("NetworkManager: added default Transport because there was none yet.");
                }
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added default Transport");
#endif
            }

            // Try and get the server first.
            server = GetComponent<NetworkServer>();
            // add NetworkServer if there is none yet. makes upgrading easier.
            if (server == null)
            {
                server = gameObject.AddComponent<NetworkServer>();
                logger.Log("NetworkManager: added NetworkServer because there was none yet.");
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(gameObject, "Added NetworkServer");
#endif
            }

            // Try and get the client first.
            client = GetComponent<NetworkClient>();
            // add NetworkClient if there is none yet. makes upgrading easier.
            if (client == null)
            {
                client = gameObject.AddComponent<NetworkClient>();
                logger.Log("NetworkManager: added NetworkClient because there was none yet.");
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
            logger.Log("Thank you for using Mirror! https://mirror-networking.com");

            // Set the networkSceneName to prevent a scene reload
            // if client connection to server fails.
            //TODO: Not sure where this goes just yet
            //networkSceneName = null;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // headless mode? then start the server
            // can't do this in Awake because Awake is for initialization.
            // some transports might not be ready until Start.
            //
            // (tick rate is applied in StartServer!)
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null && startOnHeadless)
            {
                _ = StartServer();
            }
        }

        #endregion

        #region Start & Stop

        // full server setup code, without spawning objects yet
        async Task SetupServer()
        {
            logger.Log("NetworkManager SetupServer");

            ConfigureServerFrameRate();

            // start listening to network connections
            await server.ListenAsync();
        }

        /// <summary>
        /// This starts a new server.
        /// </summary>
        /// <returns></returns>
        public async Task StartServer()
        {
            await SetupServer();

            server.SpawnObjects();
        }

        /// <summary>
        /// This starts a network client. It uses the networkAddress property as the address to connect to.
        /// <para>This makes the newly created client connect to the server immediately.</para>
        /// </summary>
        public Task StartClient(string serverIp)
        {
            if (logger.LogEnabled()) logger.Log("NetworkManager StartClient address:" + serverIp);

            var builder = new UriBuilder
            {
                Host = serverIp,
                Scheme = transport.Scheme,
            };

            return client.ConnectAsync(builder.Uri);
        }

        /// <summary>
        /// This starts a network client. It uses the Uri parameter as the address to connect to.
        /// <para>This makes the newly created client connect to the server immediately.</para>
        /// </summary>
        /// <param name="uri">location of the server to connect to</param>
        public void StartClient(Uri uri)
        {
            if (logger.LogEnabled()) logger.Log("NetworkManager StartClient address:" + uri);

            _ = client.ConnectAsync(uri);
        }

        /// <summary>
        /// This starts a network "host" - a server and client in the same application.
        /// <para>The client returned from StartHost() is a special "local" client that communicates to the in-process server using a message queue instead of the real network. But in almost all other cases, it can be treated as a normal client.</para>
        /// </summary>
        public async Task StartHost()
        {
            // setup server first
            await SetupServer();

            client.ConnectHost(server);

            // call OnStartHost AFTER SetupServer. this way we can use
            // NetworkServer.Spawn etc. in there too. just like OnStartServer
            // is called after the server is actually properly started.
            OnStartHost.Invoke();
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
            server.Disconnect();
        }

        /// <summary>
        /// Stops the client that the manager is using.
        /// </summary>
        public void StopClient()
        {
            client.Disconnect();
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
            if (!client.Active && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Application.targetFrameRate = serverTickRate;
                logger.Log("Server Tick Rate set to: " + Application.targetFrameRate + " Hz.");
            }
#endif
        }

        /// <summary>
        /// virtual so that inheriting classes' OnDestroy() can call base.OnDestroy() too
        /// </summary>
        public virtual void OnDestroy()
        {
            logger.Log("NetworkManager destroyed");
        }

        #endregion

    }
}
