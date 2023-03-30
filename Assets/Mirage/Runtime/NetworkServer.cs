using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.Events;
using Mirage.Logging;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage
{
    /// <summary>
    /// The NetworkServer.
    /// </summary>
    /// <remarks>
    /// <para>NetworkServer handles remote connections from remote clients, and also has a local connection for a local client.</para>
    /// </remarks>
    [AddComponentMenu("Network/NetworkServer")]
    [DisallowMultipleComponent]
    public class NetworkServer : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkServer));

        public bool EnablePeerMetrics;
        [Tooltip("Sequence size of buffer in bits.\n10 => array size 1024 => ~17 seconds at 60hz")]
        public int MetricsSize = 10;
        public Metrics Metrics { get; private set; }

        /// <summary>
        /// Config for peer, if not set will use default settings
        /// </summary>
        public Config PeerConfig { get; set; }

        /// <summary>
        /// The maximum number of concurrent network connections to support. Excluding the host player.
        /// <para>This field is only used if the <see cref="PeerConfig"/> property is null</para>
        /// </summary>
        [Tooltip("Maximum number of concurrent connections. Excluding the host player.\nNOTE: this field is not used if PeerConfig is set via code.")]
        [Min(1)]
        public int MaxConnections = 4;

        public bool DisconnectOnException = true;

        [Tooltip("If true will set Application.runInBackground")]
        public bool RunInBackground = true;

        [Tooltip("If disabled the server will not create a Network Peer to listen. This can be used to run server single player mode")]
        public bool Listening = true;

        [Tooltip("Creates Socket for Peer to use")]
        public SocketFactory SocketFactory;

        private Peer _peer;

        [Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator authenticator;

        [Header("Events")]
        [SerializeField] private AddLateEvent _started = new AddLateEvent();
        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// </summary>
        public IAddLateEvent Started => _started;

        /// <summary>
        /// Event fires once a new Client has connect to the Server.
        /// </summary>
        [FormerlySerializedAs("Connected")]
        [FoldoutEvent, SerializeField] private NetworkPlayerEvent _connected = new NetworkPlayerEvent();
        public NetworkPlayerEvent Connected => _connected;

        /// <summary>
        /// Event fires once a new Client has passed Authentication to the Server.
        /// </summary>
        [FormerlySerializedAs("Authenticated")]
        [FoldoutEvent, SerializeField] private NetworkPlayerEvent _authenticated = new NetworkPlayerEvent();
        public NetworkPlayerEvent Authenticated => _authenticated;

        /// <summary>
        /// Event fires once a Client has Disconnected from the Server.
        /// </summary>
        [FormerlySerializedAs("Disconnected")]
        [FoldoutEvent, SerializeField] private NetworkPlayerEvent _disconnected = new NetworkPlayerEvent();
        public NetworkPlayerEvent Disconnected => _disconnected;

        [SerializeField] private AddLateEvent _stopped = new AddLateEvent();
        public IAddLateEvent Stopped => _stopped;

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        [SerializeField] private AddLateEvent _onStartHost = new AddLateEvent();
        public IAddLateEvent OnStartHost => _onStartHost;

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        [SerializeField] private AddLateEvent _onStopHost = new AddLateEvent();
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
        public NetworkClient LocalClient { get; private set; }

        /// <summary>
        /// True if there is a local client connected to this server (host mode)
        /// </summary>
        public bool LocalClientActive => LocalClient != null && LocalClient.Active;

        /// <summary>
        /// Number of active player objects across all connections on the server.
        /// <para>This is only valid on the host / server.</para>
        /// </summary>
        // todo rename this from players to characters. (or remove it? it seems like a confusing field)
        public int NumberOfPlayers => Players.Count(kv => kv.HasCharacter);

        /// <summary>
        /// A list of local connections on the server.
        /// </summary>
        public IReadOnlyCollection<INetworkPlayer> Players => _connections.Values;

        private readonly Dictionary<IConnection, INetworkPlayer> _connections = new Dictionary<IConnection, INetworkPlayer>();

        /// <summary>
        /// <para>Checks if the server has been started.</para>
        /// <para>This will be true after NetworkServer.Listen() has been called.</para>
        /// </summary>
        public bool Active { get; private set; }

        public NetworkWorld World { get; private set; }
        public SyncVarSender SyncVarSender { get; private set; }

        private SyncVarReceiver _syncVarReceiver;
        public MessageHandler MessageHandler { get; private set; }

        /// <summary>
        /// Set to true if you want to manually call <see cref="UpdateReceive"/> and <see cref="UpdateSent"/> and stop mirage from automatically calling them
        /// </summary>
        [HideInInspector]
        public bool ManualUpdate = false;

        private void OnDestroy()
        {
            // if gameobject with server on is destroyed, stop the server
            if (Active)
                Stop();
        }

        /// <summary>
        /// This shuts down the server and disconnects all clients.
        /// <para>If In host mode, this will also stop the local client</para>
        /// </summary>
        public void Stop()
        {
            if (!Active)
            {
                logger.LogWarning("Can't stop server because it is not active");
                return;
            }

            if (LocalClient != null)
            {
                _onStopHost?.Invoke();
                LocalClient.Disconnect();
            }

            // just clear list, connections will be disconnected when peer is closed
            _connections.Clear();
            LocalPlayer = null;

            Cleanup();

            // remove listen when server is stopped so that we can cleanup correctly 
            Application.quitting -= Stop;
        }

        /// <summary>
        /// Start the server
        /// <para>If <paramref name="localClient"/> is given then will start in host mode</para>
        /// </summary>
        /// <param name="config">Config for <see cref="Peer"/></param>
        /// <param name="localClient">if not null then start the server and client in hostmode</param>
        // Has to be called "StartServer" to stop unity complaining about "Start" method
        public void StartServer(NetworkClient localClient = null)
        {
            ThrowIfActive();
            ThrowIfSocketIsMissing();

            Application.quitting += Stop;
            if (logger.LogEnabled()) logger.Log($"NetworkServer created, Mirage version: {Version.Current}");

            logger.Assert(Players.Count == 0, "Player should have been reset since previous session");
            logger.Assert(_connections.Count == 0, "Connections should have been reset since previous session");

            World = new NetworkWorld();
            SyncVarSender = new SyncVarSender();

            LocalClient = localClient;
            MessageHandler = new MessageHandler(World, DisconnectOnException);
            MessageHandler.RegisterHandler<NetworkPingMessage>(World.Time.OnServerPing);

            // create after MessageHandler, SyncVarReceiver uses it 
            _syncVarReceiver = new SyncVarReceiver(this, World);


            var dataHandler = new DataHandler(MessageHandler, _connections);
            Metrics = EnablePeerMetrics ? new Metrics(MetricsSize) : null;

            var config = PeerConfig;
            if (config == null)
            {
                config = new Config
                {
                    // only use MaxConnections if config was null
                    MaxConnections = MaxConnections,
                };
            }

            var maxPacketSize = SocketFactory.MaxPacketSize;
            NetworkWriterPool.Configure(maxPacketSize);

            // Are we listening for incoming connections?
            // If yes, set up a socket for incoming connections (we're a multiplayer game).
            // If not, that's okay. Some games use a non-listening server for their single player game mode (Battlefield, Call of Duty...)
            if (Listening)
            {
                // Create a server specific socket.
                var socket = SocketFactory.CreateServerSocket();

                // Tell the peer to use that newly created socket.
                _peer = new Peer(socket, maxPacketSize, dataHandler, config, LogFactory.GetLogger<Peer>(), Metrics);
                _peer.OnConnected += Peer_OnConnected;
                _peer.OnDisconnected += Peer_OnDisconnected;
                // Bind it to the endpoint.
                _peer.Bind(SocketFactory.GetBindEndPoint());

                if (logger.LogEnabled()) logger.Log($"Server started, listening for connections. Using socket {socket.GetType()}");

                if (RunInBackground)
                    Application.runInBackground = RunInBackground;
            }
            else
            {
                // Nicely mention that we're going live, but not listening for connections.
                if (logger.LogEnabled()) logger.Log("Server started, but not listening for connections: Attempts to connect to this instance will fail!");
            }

            InitializeAuthEvents();
            Active = true;
            _started?.Invoke();

            if (LocalClient != null)
            {
                // we should call onStartHost after transport is ready to be used
                // this allows server methods like ServerObjectManager.Spawn to be called in there
                _onStartHost?.Invoke();

                localClient.ConnectHost(this, dataHandler);
                if (logger.LogEnabled()) logger.Log("NetworkServer StartHost");
            }
        }

        private void ThrowIfActive()
        {
            if (Active) throw new InvalidOperationException("Server is already active");
        }

        private void ThrowIfSocketIsMissing()
        {
            if (SocketFactory is null)
                SocketFactory = GetComponent<SocketFactory>();
            if (SocketFactory == null)
                throw new InvalidOperationException($"{nameof(SocketFactory)} could not be found for {nameof(NetworkServer)}");
        }

        private void InitializeAuthEvents()
        {
            if (authenticator != null)
            {
                authenticator.OnServerAuthenticated += OnAuthenticated;
                authenticator.ServerSetup(this);

                Connected.AddListener(authenticator.ServerAuthenticate);
            }
            else
            {
                // if no authenticator, consider every connection as authenticated
                Connected.AddListener(OnAuthenticated);
            }
        }

        internal void Update()
        {
            if (ManualUpdate)
                return;

            UpdateReceive();
            UpdateSent();
        }

        public void UpdateReceive() => _peer?.UpdateReceive();
        public void UpdateSent()
        {
            SyncVarSender?.Update();
            _peer?.UpdateSent();
        }

        private void Peer_OnConnected(IConnection conn)
        {
            var player = new NetworkPlayer(conn);

            if (logger.LogEnabled()) logger.Log($"Server accepted client: {player}");

            // add connection
            AddConnection(player);

            // let everyone know we just accepted a connection
            Connected?.Invoke(player);
        }

        private void Peer_OnDisconnected(IConnection conn, DisconnectReason reason)
        {
            if (logger.LogEnabled()) logger.Log($"Client {conn} disconnected with reason: {reason}");

            if (_connections.TryGetValue(conn, out var player))
            {
                OnDisconnected(player);
            }
            else
            {
                // todo remove or replace with assert
                if (logger.WarnEnabled()) logger.LogWarning($"No handler found for disconnected client {conn}");
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
                Connected.RemoveListener(authenticator.ServerAuthenticate);
            }
            else
            {
                // if no authenticator, consider every connection as authenticated
                Connected.RemoveListener(OnAuthenticated);
            }

            _stopped?.Invoke();
            Active = false;

            _started.Reset();
            _onStartHost.Reset();
            _onStopHost.Reset();
            _stopped.Reset();

            World = null;
            SyncVarSender = null;

            Application.quitting -= Stop;

            if (_peer != null)
            {
                //remove handlers first to stop loop
                _peer.OnConnected -= Peer_OnConnected;
                _peer.OnDisconnected -= Peer_OnDisconnected;
                _peer.Close();
                _peer = null;
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
                _connections.Add(player.Connection, player);
            }
        }

        /// <summary>
        /// This removes an external connection.
        /// </summary>
        /// <param name="connectionId">The id of the connection to remove.</param>
        public void RemoveConnection(INetworkPlayer player)
        {
            _connections.Remove(player.Connection);
        }

        /// <summary>
        /// Create Player on Server for hostmode and adds it to collections
        /// <para>Does not invoke <see cref="Connected"/> event, use <see cref="InvokeLocalConnected"/> instead at the correct time</para>
        /// </summary>
        internal void AddLocalConnection(NetworkClient client, IConnection connection)
        {
            if (LocalPlayer != null)
            {
                throw new InvalidOperationException("Local client connection already exists");
            }

            var player = new NetworkPlayer(connection);
            LocalPlayer = player;
            LocalClient = client;

            if (logger.LogEnabled()) logger.Log($"Server accepted local client connection: {player}");

            // add the connection for this local player.
            AddConnection(player);
        }

        /// <summary>
        /// Invokes the Connected event using the local player
        /// <para>this should be done after the clients version has been invoked</para>
        /// </summary>
        internal void InvokeLocalConnected()
        {
            if (LocalPlayer == null)
            {
                throw new InvalidOperationException("Local connection does not exist");
            }
            Connected?.Invoke(LocalPlayer);
        }

        /// <summary>
        /// Send a message to all connected clients.
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="msg">Message</param>
        /// <param name="channelId">Transport channel to use</param>
        public void SendToAll<T>(T msg, Channel channelId = Channel.Reliable)
        {
            if (logger.LogEnabled()) logger.Log("Server.SendToAll id:" + typeof(T));

            using (var writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                var count = 0;

                // using SendToMany (with IEnumerable) will cause Enumerator to be boxed and create GC/alloc
                // instead we can use while loop and MoveNext to avoid boxing
                var enumerator = _connections.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var player = enumerator.Current;
                    player.Send(segment, channelId);
                    count++;
                }
                enumerator.Dispose();

                NetworkDiagnostics.OnSend(msg, segment.Count, count);
            }
        }

        /// <summary>
        /// Sends a message to many connections
        /// <para>WARNING: using this method <b>may</b> cause Enumerator to be boxed creating GC/alloc. Use <see cref="SendToMany{T}(IReadOnlyList{INetworkPlayer}, T, int)"/> version where possible</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="players"></param>
        /// <param name="msg"></param>
        /// <param name="channelId"></param>
        public static void SendToMany<T>(IEnumerable<INetworkPlayer> players, T msg, Channel channelId = Channel.Reliable)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                var count = 0;

                foreach (var player in players)
                {
                    player.Send(segment, channelId);
                    count++;
                }

                NetworkDiagnostics.OnSend(msg, segment.Count, count);
            }
        }

        /// <summary>
        /// Sends a message to many connections
        /// <para>
        /// Same as <see cref="SendToMany{T}(IEnumerable{INetworkPlayer}, T, int)"/> but uses for loop to avoid allocations
        /// </para>
        /// </summary>
        /// <remarks>
        /// Using list in foreach loop causes Unity's mono version to box the struct which causes allocations, <see href="https://docs.unity3d.com/2019.4/Documentation/Manual/BestPracticeUnderstandingPerformanceInUnity4-1.html">Understanding the managed heap</see>
        /// </remarks>
        public static void SendToMany<T>(IReadOnlyList<INetworkPlayer> players, T msg, Channel channelId = Channel.Reliable)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                var count = players.Count;

                for (var i = 0; i < count; i++)
                {
                    players[i].Send(segment, channelId);
                }

                NetworkDiagnostics.OnSend(msg, segment.Count, count);
            }
        }

        /// <summary>
        /// Sends a message to many connections, expect <paramref name="excluded"/>.
        /// <para>
        /// This can be useful if you want to send to a observers of an object expect the owner. Or if you want to send to all expect the local host player.
        /// </para>
        /// <para>WARNING: using this method <b>may</b> cause Enumerator to be boxed creating GC/alloc. Use <see cref="SendToMany{T}(IReadOnlyList{INetworkPlayer}, T, int)"/> version where possible</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="players"></param>
        /// <param name="excluded">player to exclude, Can be null</param>
        /// <param name="msg"></param>
        /// <param name="channelId"></param>
        public static void SendToManyExcept<T>(IEnumerable<INetworkPlayer> players, INetworkPlayer excluded, T msg, Channel channelId = Channel.Reliable)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                var count = 0;

                foreach (var player in players)
                {
                    if (player == excluded)
                        continue;

                    player.Send(segment, channelId);
                    count++;
                }

                NetworkDiagnostics.OnSend(msg, segment.Count, count);
            }
        }

        //called once a client disconnects from the server
        private void OnDisconnected(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("Server disconnect client:" + player);

            // set the flag first so we dont try to send any messages to the disconnected
            // connection as they wouldn't get them
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

        /// <summary>
        /// This class will later be removed when we have a better implementation for IDataHandler
        /// </summary>
        private sealed class DataHandler : IDataHandler
        {
            private readonly IMessageReceiver _messageHandler;
            private readonly Dictionary<IConnection, INetworkPlayer> _players;

            public DataHandler(IMessageReceiver messageHandler, Dictionary<IConnection, INetworkPlayer> connections)
            {
                _messageHandler = messageHandler;
                _players = connections;
            }

            public void ReceiveMessage(IConnection connection, ArraySegment<byte> message)
            {
                if (_players.TryGetValue(connection, out var player))
                {
                    _messageHandler.HandleMessage(player, message);
                }
                else
                {
                    // todo remove or replace with assert
                    if (logger.WarnEnabled()) logger.LogWarning($"No player found for message received from client {connection}");
                }
            }
        }
    }
}
