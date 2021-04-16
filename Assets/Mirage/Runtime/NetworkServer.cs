using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mirage.Events;
using Mirage.Logging;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage
{
    /// <summary>
    /// This class will later be removed when we have a better implemenation for IDataHandler
    /// </summary>
    internal class DataHandler : IDataHandler
    {
        readonly Dictionary<SocketLayer.IConnection, INetworkPlayer> players;

        public DataHandler(Dictionary<SocketLayer.IConnection, INetworkPlayer> connections)
        {
            players = connections;
        }

        public void ReceivePacket(SocketLayer.IConnection connection, ArraySegment<byte> packet)
        {
            if (players.TryGetValue(connection, out INetworkPlayer handler))
            {
                handler.HandleMessage(packet);
            }
            else
            {
                // todo remove or replace with assert
                Debug.LogWarning($"No player found for {connection}");
            }
        }
    }

    /// <summary>
    /// The NetworkServer.
    /// </summary>
    /// <remarks>
    /// <para>NetworkServer handles remote connections from remote clients, and also has a local connection for a local client.</para>
    /// </remarks>
    [AddComponentMenu("Network/NetworkServer")]
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
        /// <para>If you disable this, the server will not listen for incoming connections on the regular network port.</para>
        /// <para>This can be used if the game is running in host mode and does not want external players to be able to connect - making it like a single-player game.</para>
        /// </summary>
        public bool Listening = true;

        [Tooltip("Creates Socket for Peer to use")]
        public SocketFactory SocketFactory;

        Peer peer;
        DataHandler dataHandler;

        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator authenticator;

        [Header("Events")]
        [SerializeField] AddLateEvent _started = new AddLateEvent();
        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// </summary>
        public IAddLateEvent Started => _started;

        /// <summary>
        /// Event fires once a new Client has connect to the Server.
        /// </summary>
        [FormerlySerializedAs("Connected")]
        [FoldoutEvent, SerializeField] NetworkPlayerEvent _connected = new NetworkPlayerEvent();
        public NetworkPlayerEvent Connected => _connected;

        /// <summary>
        /// Event fires once a new Client has passed Authentication to the Server.
        /// </summary>
        [FormerlySerializedAs("Authenticated")]
        [FoldoutEvent, SerializeField] NetworkPlayerEvent _authenticated = new NetworkPlayerEvent();
        public NetworkPlayerEvent Authenticated => _authenticated;

        /// <summary>
        /// Event fires once a Client has Disconnected from the Server.
        /// </summary>
        [FormerlySerializedAs("Disconnected")]
        [FoldoutEvent, SerializeField] NetworkPlayerEvent _disconnected = new NetworkPlayerEvent();
        public NetworkPlayerEvent Disconnected => _disconnected;

        [SerializeField] AddLateEvent _stopped = new AddLateEvent();
        public IAddLateEvent Stopped => _stopped;

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        [SerializeField] AddLateEvent _onStartHost = new AddLateEvent();
        public IAddLateEvent OnStartHost => _onStartHost;

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        [SerializeField] AddLateEvent _onStopHost = new AddLateEvent();
        public IAddLateEvent OnStopHost => _onStopHost;

        /// <summary>
        /// The connection to the host mode client (if any).
        /// </summary>
        // original HLAPI has .localConnections list with only m_LocalConnection in it
        // (for backwards compatibility because they removed the real localConnections list a while ago)
        // => removed it for easier code. use .localConnection now!
        public INetworkPlayer LocalPlayer { get; private set; }

        /// <summary>
        /// The host client for this server 
        /// </summary>
        public INetworkClient LocalClient { get; private set; }

        /// <summary>
        /// True if there is a local client connected to this server (host mode)
        /// </summary>
        public bool LocalClientActive => LocalClient != null && LocalClient.Active;

        /// <summary>
        /// Number of active player objects across all connections on the server.
        /// <para>This is only valid on the host / server.</para>
        /// </summary>
        public int NumberOfPlayers => Players.Count(kv => kv.Identity != null);

        /// <summary>
        /// A list of local connections on the server.
        /// </summary>
        public readonly HashSet<INetworkPlayer> Players = new HashSet<INetworkPlayer>();
        readonly Dictionary<SocketLayer.IConnection, INetworkPlayer> connections = new Dictionary<SocketLayer.IConnection, INetworkPlayer>();

        IReadOnlyCollection<INetworkPlayer> INetworkServer.Players => Players;

        /// <summary>
        /// <para>Checks if the server has been started.</para>
        /// <para>This will be true after NetworkServer.Listen() has been called.</para>
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Time kept in this server
        /// </summary>
        public NetworkTime Time { get; } = new NetworkTime();

        public NetworkWorld World { get; private set; }

        /// <summary>
        /// This shuts down the server and disconnects all clients.
        /// <para>If In host mode, this will also stop the local client</para>
        /// </summary>
        public void Stop()
        {
            if (LocalClient != null)
            {
                _onStopHost?.Invoke();
                LocalClient.Disconnect();
            }

            // just clear list, connections will be disconnected when peer is closed
            Players.Clear();
            LocalPlayer = null;

            Cleanup();

            // remove listen when server is stopped so that 
            Application.quitting -= Stop;
        }

        void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            World = new NetworkWorld();

            Application.quitting += Stop;
            if (logger.LogEnabled()) logger.Log($"NetworkServer Created, Mirage version: {Version.Current}");


            //Make sure connections are cleared in case any old connections references exist from previous sessions
            Players.Clear();
            connections.Clear();

            if (SocketFactory is null)
                SocketFactory = GetComponent<SocketFactory>();
            if (SocketFactory == null)
                throw new InvalidOperationException($"{nameof(SocketFactory)} could not be found for ${nameof(NetworkServer)}");

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

        /// <summary>
        /// Start the server
        /// <para>If <paramref name="localClient"/> is given then will start in host mode</para>
        /// </summary>
        /// <param name="localClient">if not null then start the server and client in hostmode</param>
        /// <returns></returns>
        public void Start(NetworkClient localClient = null)
        {
            if (Active) throw new InvalidOperationException("Server is already active");

            LocalClient = localClient;

            Initialize();

            CreateAndBindSocket();

            if (LocalClient != null)
            {
                localClient.ConnectHost(this, dataHandler);
                logger.Log("NetworkServer StartHost");
            }
        }

        void CreateAndBindSocket()
        {
            ISocket socket = SocketFactory.CreateServerSocket();
            dataHandler = new DataHandler(connections);
            ILogger peerLogger = LogFactory.GetLogger<Peer>();
            peer = new Peer(socket, dataHandler, logger: peerLogger);
            EndPoint endpoint = SocketFactory.GetBindEndPoint();

            peer.OnConnected += Peer_OnConnected;
            peer.OnDisconnected += Peer_OnDisconnected;
            peer.Bind(endpoint);

            TransportStarted();
        }
        private void Update()
        {
            peer?.Update();
        }


        private void Peer_OnConnected(SocketLayer.IConnection conn)
        {
            var networkConnectionToClient = new NetworkPlayer(conn);
            ConnectionAccepted(networkConnectionToClient);
        }

        private void Peer_OnDisconnected(SocketLayer.IConnection conn, DisconnectReason reason)
        {
            if (logger.LogEnabled()) logger.Log($"[{conn}] discconnected with reason {reason}");

            if (connections.TryGetValue(conn, out INetworkPlayer player))
            {
                OnDisconnected(player);
            }
            else
            {
                // todo remove or replace with assert
                Debug.LogWarning("no handler found for connection");
            }
        }

        private void TransportStarted()
        {
            logger.Log("Server started listening");
            Active = true;
            // (useful for loading & spawning stuff from database etc.)
            _started?.Invoke();

            if (LocalClient != null)
            {
                // we should call onStartHost after transport is ready to be used
                // this allows server methods like NetworkServer.Spawn to be called in there
                _onStartHost?.Invoke();
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

            _stopped?.Invoke();
            initialized = false;
            Active = false;

            _started.Reset();
            _onStartHost.Reset();
            _onStopHost.Reset();
            _stopped.Reset();

            Application.quitting -= Stop;

            if (peer != null)
            {
                //remove handlers first to stop loop
                peer.OnConnected -= Peer_OnConnected;
                peer.OnDisconnected -= Peer_OnDisconnected;
                peer.Close();
                peer = null;
            }
        }

        /// <summary>
        /// <para>This accepts a network connection and adds it to the server.</para>
        /// <para>This connection will use the callbacks registered with the server.</para>
        /// </summary>
        /// <param name="player">Network connection to add.</param>
        public void AddConnection(INetworkPlayer player)
        {
            if (!Players.Contains(player))
            {
                // connection cannot be null here or conn.connectionId
                // would throw NRE
                Players.Add(player);
                connections.Add(player.Connection, player);
                player.RegisterHandler<NetworkPingMessage>(Time.OnServerPing);
            }
        }

        /// <summary>
        /// This removes an external connection.
        /// </summary>
        /// <param name="connectionId">The id of the connection to remove.</param>
        public void RemoveConnection(INetworkPlayer player)
        {
            Players.Remove(player);
            connections.Remove(player.Connection);
        }

        /// <summary>
        /// called by LocalClient to add itself. dont call directly.
        /// </summary>
        /// <param name="client">The local client</param>
        /// <param name="connection">The connection to the client</param>
        internal void SetLocalConnection(INetworkClient client, SocketLayer.IConnection connection)
        {
            if (LocalPlayer != null)
            {
                throw new InvalidOperationException("Local Connection already exists");
            }

            var player = new NetworkPlayer(connection);
            LocalPlayer = player;
            LocalClient = client;

            ConnectionAccepted(player);
        }

        /// <summary>
        /// Send a message to all connected clients.
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="msg">Message</param>
        /// <param name="channelId">Transport channel to use</param>
        public void SendToAll<T>(T msg, int channelId = Channel.Reliable)
        {
            if (logger.LogEnabled()) logger.Log("Server.SendToAll id:" + typeof(T));
            SendToMany(Players, msg, channelId);
        }

        public static void SendToMany<T>(IEnumerable<INetworkPlayer> players, T msg, int channelId = Channel.Reliable)
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                int count = 0;

                foreach (INetworkPlayer player in players)
                {
                    // send to all connections, but don't wait for them
                    player.Send(segment, channelId);
                    count++;
                }

                NetworkDiagnostics.OnSend(msg, channelId, segment.Count, count);
            }
        }

        void ConnectionAccepted(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("Server accepted client:" + player);

            //Only allow host client to connect when not Listening for new connections
            if (!Listening && player != LocalPlayer)
            {
                return;
            }

            // are more connections allowed? if not, kick
            // (it's easier to handle this in Mirage, so Transports can have
            //  less code and third party transport might not do that anyway)
            // (this way we could also send a custom 'tooFull' message later,
            //  Transport can't do that)
            if (Players.Count >= MaxConnections)
            {
                player.Connection?.Disconnect();
                if (logger.WarnEnabled()) logger.LogWarning("Server full, kicked client:" + player);
                return;
            }

            // add connection
            AddConnection(player);

            // let everyone know we just accepted a connection
            Connected?.Invoke(player);
        }

        //called once a client disconnects from the server
        void OnDisconnected(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("Server disconnect client:" + player);

            // set flag first so we dont try to send message to connection
            player.MarkAsDisconnected();

            RemoveConnection(player);

            Disconnected?.Invoke(player);

            player.DestroyOwnedObjects();
            player.Identity = null;

            if (player == LocalPlayer)
                LocalPlayer = null;
        }

        internal void OnAuthenticated(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("Server authenticate client:" + player);

            Authenticated?.Invoke(player);
        }
    }
}
