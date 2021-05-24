using System;
using System.Net;
using Mirage.Events;
using Mirage.Logging;
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
        public Metrics Metrics { get; private set; }

        [Tooltip("Creates Socket for Peer to use")]
        public SocketFactory SocketFactory;

        Peer peer;

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
        /// Event fires after the Client connection has successfully been authenticated with its Server.
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
        /// <param name="uri">Address of the server to connect to</param>
        public void Connect(string address = null, ushort? port = null)
        {

            if (SocketFactory is null)
                SocketFactory = GetComponent<SocketFactory>();
            if (SocketFactory == null)
                throw new InvalidOperationException($"{nameof(SocketFactory)} could not be found for ${nameof(NetworkClient)}");

            connectState = ConnectState.Connecting;

            EndPoint endPoint = SocketFactory.GetConnectEndPoint(address, port);

            if (logger.LogEnabled()) logger.Log($"Client connecting to endpoint: {endPoint}");

            try
            {
                ISocket socket = SocketFactory.CreateClientSocket();
                var dataHandler = new DataHandler();
                Metrics = EnablePeerMetrics ? new Metrics() : null;
                peer = new Peer(socket, dataHandler, logger: LogFactory.GetLogger<Peer>(), metrics: Metrics);

                peer.OnConnected += Peer_OnConnected;
                peer.OnConnectionFailed += Peer_OnConnectionFailed;
                peer.OnDisconnected += Peer_OnDisconnected;
                IConnection connection = peer.Connect(endPoint);

                // todo do we initialize now or after connected
                World = new NetworkWorld();
                InitializeAuthEvents();

                // setup all the handlers
                Player = new NetworkPlayer(connection);
                dataHandler.SetConnection(connection, Player);
                Time.Reset();

                RegisterMessageHandlers();
            }
            catch (Exception)
            {
                connectState = ConnectState.Disconnected;
                throw;
            }
        }

        private void Peer_OnConnected(IConnection conn)
        {
            Time.UpdateClient(this);
            connectState = ConnectState.Connected;
            _connected.Invoke(Player);
        }

        private void Peer_OnConnectionFailed(IConnection conn, RejectReason reason)
        {
            if (logger.LogEnabled()) logger.Log($"Failed to connect to {conn.EndPoint} with reason {reason}");
            Player.MarkAsDisconnected();
            // todo add connection failed event
            _disconnected?.Invoke();
            Cleanup();
        }

        private void Peer_OnDisconnected(IConnection conn, DisconnectReason reason)
        {
            if (logger.LogEnabled()) logger.Log($"Disconnected from {conn.EndPoint} with reason {reason}");
            Player?.MarkAsDisconnected();
            // todo add reason to disconnected event
            //     use different enum, so that:
            //     - user doesn't need to add reference to socket layer
            //     - add high level reason so that they are easier to understand by user
            _disconnected?.Invoke();
            Cleanup();
        }

        internal void ConnectHost(NetworkServer server, IDataHandler serverDataHandler)
        {
            logger.Log("Client Connect Host to Server");
            connectState = ConnectState.Connected;

            World = server.World;
            InitializeAuthEvents();

            // create local connection objects and connect them
            var dataHandler = new DataHandler();
            (IConnection clientConn, IConnection serverConn) = PipePeerConnection.Create(dataHandler, serverDataHandler);

            // set up client before connecting to server, server could invoke handlers
            IsLocalClient = true;
            Player = new NetworkPlayer(clientConn);
            dataHandler.SetConnection(clientConn, Player);
            RegisterHostHandlers();

            // client has to connect first or it will miss message in NetworkScenemanager
            Peer_OnConnected(clientConn);

            server.SetLocalConnection(this, serverConn);
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
            // todo exit early if not active/initialized

            Player?.Connection?.Disconnect();
            _disconnected?.Invoke();
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
                Time.UpdateClient(this);
            }
            peer?.Update();
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

            public void SetConnection(IConnection connection, INetworkPlayer player)
            {
                this.connection = connection;
                this.player = player;
            }

            public void ReceiveMessage(IConnection connection, ArraySegment<byte> message)
            {
                logger.Assert(this.connection == connection);
                player.HandleMessage(message);
            }
        }
    }
}
