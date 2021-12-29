using System;
using Mirage.Events;
using Mirage.Logging;
using Mirage.Serialization;
using Mirage.SocketLayer;
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

        public bool EnablePeerMetrics;
        [Tooltip("Sequence size of buffer in bits.\n10 => array size 1024 => ~17 seconds at 60hz")]
        public int MetricsSize = 10;
        public Metrics Metrics { get; private set; }

        /// <summary>
        /// Config for peer, if not set will use default settings
        /// </summary>
        public Config PeerConfig { get; set; }

        [Tooltip("Creates Socket for Peer to use")]
        public SocketFactory SocketFactory;

        public bool DisconnectOnException = true;

        Peer peer;

        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator authenticator;

        [Header("Events")]
        [SerializeField] AddLateEvent _started = new AddLateEvent();
        [SerializeField] NetworkPlayerAddLateEvent _connected = new NetworkPlayerAddLateEvent();
        [SerializeField] NetworkPlayerAddLateEvent _authenticated = new NetworkPlayerAddLateEvent();
        [SerializeField] DisconnectAddLateEvent _disconnected = new DisconnectAddLateEvent();

        /// <summary>
        /// Event fires when the client starts, before it has connected to the Server.
        /// </summary>
        public IAddLateEvent Started => _started;

        /// <summary>
        /// Event fires once the Client has connected its Server.
        /// </summary>
        public IAddLateEvent<INetworkPlayer> Connected => _connected;

        /// <summary>
        /// Event fires after the Client connection has successfully been authenticated with its Server.
        /// </summary>
        public IAddLateEvent<INetworkPlayer> Authenticated => _authenticated;

        /// <summary>
        /// Event fires after the Client has disconnected from its Server and Cleanup has been called.
        /// </summary>
        public IAddLateEvent<ClientStoppedReason> Disconnected => _disconnected;

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

        public NetworkWorld World { get; private set; }
        public MessageHandler MessageHandler { get; private set; }


        /// <summary>
        /// NetworkClient can connect to local server in host mode too
        /// </summary>
        public bool IsLocalClient { get; private set; }

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void Connect(string address = null, ushort? port = null)
        {
            ThrowIfActive();
            ThrowIfSocketIsMissing();

            connectState = ConnectState.Connecting;

            World = new NetworkWorld();

            IEndPoint endPoint = SocketFactory.GetConnectEndPoint(address, port);
            if (logger.LogEnabled()) logger.Log($"Client connecting to endpoint: {endPoint}");

            ISocket socket = SocketFactory.CreateClientSocket();
            MessageHandler = new MessageHandler(World, DisconnectOnException);
            var dataHandler = new DataHandler(MessageHandler);
            Metrics = EnablePeerMetrics ? new Metrics(MetricsSize) : null;

            Config config = PeerConfig ?? new Config();

            NetworkWriterPool.Configure(config.MaxPacketSize);

            peer = new Peer(socket, dataHandler, config, LogFactory.GetLogger<Peer>(), Metrics);
            peer.OnConnected += Peer_OnConnected;
            peer.OnConnectionFailed += Peer_OnConnectionFailed;
            peer.OnDisconnected += Peer_OnDisconnected;

            IConnection connection = peer.Connect(endPoint);

            // setup all the handlers
            Player = new NetworkPlayer(connection);
            dataHandler.SetConnection(connection, Player);

            RegisterMessageHandlers();
            InitializeAuthEvents();
            // invoke started event after everything is set up, but before peer has connected
            _started.Invoke();
        }

        void ThrowIfActive()
        {
            if (Active) throw new InvalidOperationException("Client is already active");
        }

        void ThrowIfSocketIsMissing()
        {
            if (SocketFactory is null)
                SocketFactory = GetComponent<SocketFactory>();
            if (SocketFactory == null)
                throw new InvalidOperationException($"{nameof(SocketFactory)} could not be found for ${nameof(NetworkServer)}");
        }

        private void Peer_OnConnected(IConnection conn)
        {
            World.Time.UpdateClient(this);
            connectState = ConnectState.Connected;
            _connected.Invoke(Player);
        }

        private void Peer_OnConnectionFailed(IConnection conn, RejectReason reason)
        {
            if (logger.LogEnabled()) logger.Log($"Failed to connect to {conn.EndPoint} with reason {reason}");
            Player?.MarkAsDisconnected();
            _disconnected?.Invoke(reason.ToClientStoppedReason());
            Cleanup();
        }

        private void Peer_OnDisconnected(IConnection conn, DisconnectReason reason)
        {
            if (logger.LogEnabled()) logger.Log($"Disconnected from {conn.EndPoint} with reason {reason}");
            Player?.MarkAsDisconnected();
            _disconnected?.Invoke(reason.ToClientStoppedReason());
            Cleanup();
        }

        void OnHostDisconnected()
        {
            Player?.MarkAsDisconnected();
            _disconnected?.Invoke(ClientStoppedReason.HostModeStopped);
        }

        internal void ConnectHost(NetworkServer server, IDataHandler serverDataHandler)
        {
            logger.Log("Client Connect Host to Server");
            // start connecting for setup, then "Peer_OnConnected" below will change to connected
            connectState = ConnectState.Connecting;

            World = server.World;

            // create local connection objects and connect them
            MessageHandler = new MessageHandler(World, DisconnectOnException);
            var dataHandler = new DataHandler(MessageHandler);
            (IConnection clientConn, IConnection serverConn) = PipePeerConnection.Create(dataHandler, serverDataHandler, OnHostDisconnected, null);

            // set up client before connecting to server, server could invoke handlers
            IsLocalClient = true;
            Player = new NetworkPlayer(clientConn);
            dataHandler.SetConnection(clientConn, Player);
            RegisterHostHandlers();
            InitializeAuthEvents();
            // invoke started event after everything is set up, but before peer has connected
            _started.Invoke();

            // we need add server connection to server's dictionary first
            // then invoke connected event on client (client has to connect first or it will miss message in NetworkScenemanager)
            // then invoke connected event on server

            server.AddLocalConnection(this, serverConn);
            Peer_OnConnected(clientConn);
            server.InvokeLocalConnected();
        }

        void InitializeAuthEvents()
        {
            if (authenticator != null)
            {
                authenticator.OnClientAuthenticated += OnAuthenticated;
                authenticator.ClientSetup(this);

                Connected.AddListener(authenticator.ClientAuthenticate);
            }
            else
            {
                // if no authenticator, consider connection as authenticated
                Connected.AddListener(OnAuthenticated);
            }
        }

        internal void OnAuthenticated(INetworkPlayer player)
        {
            _authenticated.Invoke(player);
        }

        private void OnDestroy()
        {
            if (Active)
                Disconnect();
        }

        /// <summary>
        /// Disconnect from server.
        /// <para>The disconnect message will be invoked.</para>
        /// </summary>
        public void Disconnect()
        {
            if (!Active)
            {
                logger.LogWarning("Can't disconnect client because it is not active");
                return;
            }

            Player.Connection.Disconnect();
            Cleanup();
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
                World.Time.UpdateClient(this);
            }
            peer?.UpdateReceive();
            peer?.UpdateSent();
        }

        internal void RegisterHostHandlers()
        {
            MessageHandler.RegisterHandler<NetworkPongMessage>(msg => { });
        }

        internal void RegisterMessageHandlers()
        {
            MessageHandler.RegisterHandler<NetworkPongMessage>(World.Time.OnClientPong);
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
                Connected.RemoveListener(authenticator.ClientAuthenticate);
            }
            else
            {
                // if no authenticator, consider connection as authenticated
                Connected.RemoveListener(OnAuthenticated);
            }

            Player = null;
            _connected.Reset();
            _authenticated.Reset();
            _disconnected.Reset();

            if (peer != null)
            {
                //remove handlers first to stop loop
                peer.OnConnected -= Peer_OnConnected;
                peer.OnConnectionFailed -= Peer_OnConnectionFailed;
                peer.OnDisconnected -= Peer_OnDisconnected;
                peer.Close();
                peer = null;
            }
        }

        internal class DataHandler : IDataHandler
        {
            IConnection connection;
            INetworkPlayer player;
            readonly IMessageReceiver messageHandler;

            public DataHandler(IMessageReceiver messageHandler)
            {
                this.messageHandler = messageHandler;
            }

            public void SetConnection(IConnection connection, INetworkPlayer player)
            {
                this.connection = connection;
                this.player = player;
            }

            public void ReceiveMessage(IConnection connection, ArraySegment<byte> message)
            {
                logger.Assert(this.connection == connection);
                messageHandler.HandleMessage(player, message);
            }
        }
    }
}
