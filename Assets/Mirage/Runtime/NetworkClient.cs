using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Events;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{


    public enum ConnectState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    /// <summary>
    /// This is a network client class used by the networking system. It contains a NetworkConnection that is used to connect to a network server.
    /// <para>The <see cref="NetworkClient">NetworkClient</see> handle connection state, messages handlers, and connection configuration. There can be many <see cref="NetworkClient">NetworkClient</see> instances in a process at a time, but only one that is connected to a game server (<see cref="NetworkServer">NetworkServer</see>) that uses spawned objects.</para>
    /// <para><see cref="NetworkClient">NetworkClient</see> has an internal update function where it handles events from the transport layer. This includes asynchronous connect events, disconnect events and incoming data from a server.</para>
    /// </summary>
    [AddComponentMenu("Network/NetworkClient")]
    [DisallowMultipleComponent]
    public class NetworkClient : MonoBehaviour, INetworkClient
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkClient));

        public Transport Transport;

        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator authenticator;

        [Header("Events")]
        [SerializeField] NetworkPlayerAddLateEvent _connected = new NetworkPlayerAddLateEvent();
        [SerializeField] NetworkPlayerAddLateEvent _authenticated = new NetworkPlayerAddLateEvent();
        [SerializeField] AddLateEvent _disconnected = new AddLateEvent();

        /// <summary>
        /// Event fires once the Client has connected its Server.
        /// </summary>
        public IAddLateEvent<INetworkPlayer> Connected => _connected;

        /// <summary>
        /// Event fires after the Client connection has sucessfully been authenticated with its Server.
        /// </summary>
        public IAddLateEvent<INetworkPlayer> Authenticated => _authenticated;

        /// <summary>
        /// Event fires after the Client has disconnected from its Server and Cleanup has been called.
        /// </summary>
        public IAddLateEvent Disconnected => _disconnected;

        /// <summary>
        /// The NetworkConnection object this client is using.
        /// </summary>
        public INetworkPlayer Player { get; internal set; }

        internal ConnectState connectState = ConnectState.Disconnected;

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
        /// Time kept in this client
        /// </summary>
        public NetworkTime Time { get; } = new NetworkTime();

        public NetworkWorld World { get; private set; }

        /// <summary>
        /// NetworkClient can connect to local server in host mode too
        /// </summary>
        public bool IsLocalClient { get; private set; }

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="serverIp">Address of the server to connect to</param>
        public UniTask ConnectAsync(string serverIp)
        {
            if (logger.LogEnabled()) logger.Log("Client address:" + serverIp);

            var builder = new UriBuilder
            {
                Host = serverIp,
                Scheme = Transport.Scheme.First(),
            };

            return ConnectAsync(builder.Uri);
        }

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="serverIp">Address of the server to connect to</param>
        /// <param name="port">The port of the server to connect to</param>
        public UniTask ConnectAsync(string serverIp, ushort port)
        {
            if (logger.LogEnabled()) logger.Log("Client address and port:" + serverIp + ":" + port);

            var builder = new UriBuilder
            {
                Host = serverIp,
                Port = port,
                Scheme = Transport.Scheme.First()
            };

            return ConnectAsync(builder.Uri);
        }

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="uri">Address of the server to connect to</param>
        public async UniTask ConnectAsync(Uri uri)
        {
            if (logger.LogEnabled()) logger.Log("Client Connect: " + uri);

            if (Transport == null)
                Transport = GetComponent<Transport>();
            if (Transport == null)
                throw new InvalidOperationException("Transport could not be found for NetworkClient");

            connectState = ConnectState.Connecting;

            try
            {
                IConnection transportConnection = await Transport.ConnectAsync(uri);

                World = new NetworkWorld();
                InitializeAuthEvents();

                // setup all the handlers
                Player = GetNewPlayer(transportConnection);
                Time.Reset();

                RegisterMessageHandlers();
                Time.UpdateClient(this);
                OnConnected().Forget();
            }
            catch (Exception)
            {
                connectState = ConnectState.Disconnected;
                throw;
            }
        }

        internal void ConnectHost(NetworkServer server)
        {
            logger.Log("Client Connect Host to Server");
            connectState = ConnectState.Connected;

            World = server.World;
            InitializeAuthEvents();

            // create local connection objects and connect them
            (IConnection c1, IConnection c2) = PipeConnection.CreatePipe();

            server.SetLocalConnection(this, c2);
            IsLocalClient = true;
            Player = GetNewPlayer(c1);
            RegisterHostHandlers();

            OnConnected().Forget();
        }

        /// <summary>
        /// Creates a new INetworkConnection based on the provided IConnection.
        /// </summary>
        public virtual INetworkPlayer GetNewPlayer(IConnection connection)
        {
            return new NetworkPlayer(connection);
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

        async UniTaskVoid OnConnected()
        {
            // reset network time stats

            // the handler may want to send messages to the client
            // thus we should set the connected state before calling the handler
            connectState = ConnectState.Connected;
            _connected.Invoke(Player);

            // start processing messages
            try
            {
                await Player.ProcessMessagesAsync();
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                Cleanup();

                _disconnected?.Invoke();
            }
        }

        internal void OnAuthenticated(INetworkPlayer player)
        {
            _authenticated.Invoke(player);
        }

        /// <summary>
        /// Disconnect from server.
        /// <para>The disconnect message will be invoked.</para>
        /// </summary>
        public void Disconnect()
        {
            Player?.Connection?.Disconnect();
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
        public void Send<T>(T message, int channelId = Channel.Reliable)
        {
            Player.Send(message, channelId);
        }

        public void Send(ArraySegment<byte> segment, int channelId = Channel.Reliable)
        {
            Player.Send(segment, channelId);
        }

        internal void Update()
        {
            // local connection?
            if (!IsLocalClient && Active && connectState == ConnectState.Connected)
            {
                // only update things while connected
                Time.UpdateClient(this);
            }
        }

        internal void RegisterHostHandlers()
        {
            Player.RegisterHandler<NetworkPongMessage>(msg => { });
        }

        internal void RegisterMessageHandlers()
        {
            Player.RegisterHandler<NetworkPongMessage>(Time.OnClientPong);
        }


        /// <summary>
        /// Shut down a client.
        /// <para>This should be done when a client is no longer going to be used.</para>
        /// </summary>
        void Cleanup()
        {
            logger.Log("Shutting down client.");

            IsLocalClient = false;

            connectState = ConnectState.Disconnected;

            if (authenticator != null)
            {
                authenticator.OnClientAuthenticated -= OnAuthenticated;
                Connected.RemoveListener(authenticator.OnClientAuthenticateInternal);
            }
            else
            {
                // if no authenticator, consider connection as authenticated
                Connected.RemoveListener(OnAuthenticated);
            }

            _connected.Reset();
            _authenticated.Reset();
            _disconnected.Reset();
        }
    }
}
