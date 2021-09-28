using System;
using System.Collections.Generic;
using Mirage.Logging;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;
using Mirage.Events;

namespace Mirage.Core
{
    /// <summary>
    /// Server object.
    /// </summary>
    /// <remarks>
    /// <para>Server handles connections from remote clients, and also has a local connection for a local client.</para>
    /// </remarks>
    public class Server : INetworkServer
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(Server));

        public SocketFactory SocketFactory;
        public NetworkAuthenticator Authenticator;

        public int MaxConnections => maxConnections;

        /// <summary>
        /// A list of local connections on the server.
        /// </summary>
        public IReadOnlyCollection<INetworkPlayer> Players => players;
        public NetworkWorld World { get; private set; }

        public SyncVarSender SyncVarSender { get; private set; }
        public MessageHandler MessageHandler { get; private set; }

        /// <summary>
        /// The host client for this server
        /// </summary>
        public INetworkClient LocalClient { get; private set; }

        public bool LocalClientActive => LocalClient != null && LocalClient.Active;
        public Metrics Metrics { get; private set; }
        public Config PeerConfig { get; set; }

        /// <summary>
        /// <para>Checks if the server has been started.</para>
        /// <para>This will be true after NetworkServer.Listen() has been called.</para>
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// The connection to the host mode client (if any).
        /// </summary>
        // original HLAPI has .localConnections list with only m_LocalConnection in it
        // (for backwards compatibility because they removed the real localConnections list a while ago)
        // => removed it for easier code. use .localConnection now!
        public INetworkPlayer LocalPlayer { get; private set; }
        public IAddLateEvent Started { get; set; }
        public IAddLateEvent OnStartHost { get; set; }
        public IAddLateEvent OnStopHost { get; set; }
        public NetworkPlayerEvent Connected { get; set; }
        public IAddLateEvent Stopped { get; set; }
        public NetworkPlayerEvent Authenticated { get; set; }
        public NetworkPlayerEvent Disconnected { get; set; }

        /// <summary>
        /// Number of active player objects across all connections on the server.
        /// <para>This is only valid on the host / server.</para>
        /// </summary>
        public int NumberOfPlayers => Players.Count(kv => kv.HasCharacter);

        readonly Dictionary<IConnection, INetworkPlayer> connections = new Dictionary<IConnection, INetworkPlayer>();
        readonly HashSet<INetworkPlayer> players = new HashSet<INetworkPlayer>();
        readonly bool enablePeerMetrics;
        readonly int metricsSize;
        readonly bool disconnectOnException;
        readonly bool listening;
        readonly int maxConnections;
        Peer peer;

        public Server(ServerConfig config, ServerEvents events)
        {
            SocketFactory = config.SocketFactory;
            enablePeerMetrics = config.EnablePeerMetrics;
            metricsSize = config.MetricsSize;
            disconnectOnException = config.DisconnectOnException;
            listening = config.Listening;
            maxConnections = config.MaxConnections;

            Started = events.Started;
            Stopped = events.Stopped;
            Authenticated = events.Authenticated;
            OnStartHost = events.OnStartHost;
            OnStopHost = events.OnStopHost;
            Connected = events.Connected;
            Disconnected = events.Disconnected;
        }

        public void StartServer(NetworkClient localClient = null)
        {
            ThrowIfActive();
            ThrowIfSocketIsMissing();

            if (logger.LogEnabled()) logger.Log($"NetworkServer Created, Mirage version: {Version.Current}");

            logger.Assert(Players.Count == 0, "Player should have been reset since previous session");
            logger.Assert(connections.Count == 0, "Connections should have been reset since previous session");

            World = new NetworkWorld();
            SyncVarSender = new SyncVarSender();

            LocalClient = localClient;
            MessageHandler = new MessageHandler(World, disconnectOnException);
            MessageHandler.RegisterHandler<NetworkPingMessage>(World.Time.OnServerPing);

            ISocket socket = SocketFactory.CreateServerSocket();
            var dataHandler = new DataHandler(MessageHandler, connections);
            Metrics = enablePeerMetrics ? new Metrics(metricsSize) : null;
            Config config = PeerConfig ?? new Config
            {
                MaxConnections = maxConnections,
            };

            NetworkWriterPool.Configure(config.MaxPacketSize);

            peer = new Peer(socket, dataHandler, config, LogFactory.GetLogger<Peer>(), Metrics);
            peer.OnConnected += Peer_OnConnected;
            peer.OnDisconnected += Peer_OnDisconnected;

            peer.Bind(SocketFactory.GetBindEndPoint());

            if (logger.LogEnabled()) logger.Log("Server started listening");

            InitializeAuthEvents();
            Active = true;
            Started.Invoke();

            if (LocalClient != null)
            {
                // we should call onStartHost after transport is ready to be used
                // this allows server methods like NetworkServer.Spawn to be called in there
                OnStartHost.Invoke();

                localClient.ConnectHost(this, dataHandler);
                if (logger.LogEnabled()) logger.Log("NetworkServer StartHost");
            }
        }

        public void Update()
        {
            peer?.Update();
            SyncVarSender?.Update();
        }

        public void Stop()
        {
            if (!Active)
            {
                logger.LogWarning("Can't stop server because it is not active");
                return;
            }

            if (LocalClient != null)
            {
                OnStopHost.Invoke();
                LocalClient.Disconnect();
            }

            // just clear list, connections will be disconnected when peer is closed
            players.Clear();
            connections.Clear();
            LocalPlayer = null;

            Cleanup();
        }

        /// <summary>
        /// Create Player on Server for hostmode and adds it to collections
        /// <para>Does not invoke <see cref="Connected"/> event, use <see cref="InvokeLocalConnected"/> instead at the correct time</para>
        /// </summary>
        internal void AddLocalConnection(INetworkClient client, IConnection connection)
        {
            if (LocalPlayer != null)
            {
                throw new InvalidOperationException("Local Connection already exists");
            }

            var player = new NetworkPlayer(connection);
            LocalPlayer = player;
            LocalClient = client;

            if (logger.LogEnabled()) logger.Log("Server accepted Local client:" + player);

            // add connection
            AddConnection(player);
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
                players.Add(player);
                connections.Add(player.Connection, player);
            }
        }

        /// <summary>
        /// This removes an external connection.
        /// </summary>
        /// <param name="connectionId">The id of the connection to remove.</param>
        public void RemoveConnection(INetworkPlayer player)
        {
            players.Remove(player);
            connections.Remove(player.Connection);
        }

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
                    player.Send(segment, channelId);
                    count++;
                }

                NetworkDiagnostics.OnSend(msg, segment.Count, count);
            }
        }


        void ThrowIfActive()
        {
            if (Active) throw new InvalidOperationException("Server is already active");
        }

        void ThrowIfSocketIsMissing()
        {
            if (SocketFactory == null)
            {
                throw new InvalidOperationException($"{nameof(SocketFactory)} could not be found for {nameof(Server)}");
            }
        }

        void InitializeAuthEvents()
        {
            if (Authenticator != null)
            {
                Authenticator.OnServerAuthenticated += OnAuthenticated;
                Authenticator.ServerSetup(this);

                Connected.AddListener(Authenticator.ServerAuthenticate);
            }
            else
            {
                // if no authenticator, consider every connection as authenticated
                Connected.AddListener(OnAuthenticated);
            }
        }

        private void Peer_OnConnected(IConnection conn)
        {
            var player = new NetworkPlayer(conn);
            ConnectionAccepted(player);
        }

        private void Peer_OnDisconnected(IConnection conn, DisconnectReason reason)
        {
            if (logger.LogEnabled()) logger.Log($"[{conn}] discconnected with reason {reason}");

            if (connections.TryGetValue(conn, out INetworkPlayer player))
            {
                OnDisconnected(player);
            }
            else
            {
                // todo remove or replace with assert
                if (logger.WarnEnabled()) logger.LogWarning($"No handler found for [{conn}]");
            }
        }

        /// <summary>
        /// cleanup resources so that we can start again
        /// </summary>
        private void Cleanup()
        {
            if (Authenticator != null)
            {
                Authenticator.OnServerAuthenticated -= OnAuthenticated;
                Connected.RemoveListener(Authenticator.ServerAuthenticate);
            }
            else
            {
                // if no authenticator, consider every connection as authenticated
                Connected.RemoveListener(OnAuthenticated);
            }

            Stopped?.Invoke();
            Active = false;

            World = null;
            SyncVarSender = null;

            if (peer != null)
            {
                //remove handlers first to stop loop
                peer.OnConnected -= Peer_OnConnected;
                peer.OnDisconnected -= Peer_OnDisconnected;
                peer.Close();
                peer = null;
            }

            Started.Reset();
            OnStartHost.Reset();
            OnStopHost.Reset();
            Stopped.Reset();
        }

        internal void OnAuthenticated(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("Server authenticate client:" + player);

            Authenticated?.Invoke(player);
        }

        /// <summary>
        /// Invokes the Connected event using the local player
        /// <para>this should be done after the clients version has been invoked</para>
        /// </summary>
        internal void InvokeLocalConnected()
        {
            if (LocalPlayer == null)
            {
                throw new InvalidOperationException("Local Connection does not exist");
            }
            Connected?.Invoke(LocalPlayer);
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

        void ConnectionAccepted(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("Server accepted client:" + player);

            //Only allow host client to connect when not Listening for new connections
            if (!listening && player != LocalPlayer)
            {
                return;
            }

            // are more connections allowed? if not, kick
            // (it's easier to handle this in Mirage, so Transports can have
            //  less code and third party transport might not do that anyway)
            // (this way we could also send a custom 'tooFull' message later,
            //  Transport can't do that)
            if (Players.Count >= maxConnections)
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


        /// <summary>
        /// This class will later be removed when we have a better implementation for IDataHandler
        /// </summary>
        class DataHandler : IDataHandler
        {
            readonly IMessageReceiver messageHandler;
            readonly Dictionary<IConnection, INetworkPlayer> players;

            public DataHandler(IMessageReceiver messageHandler, Dictionary<IConnection, INetworkPlayer> connections)
            {
                this.messageHandler = messageHandler;
                players = connections;
            }

            public void ReceiveMessage(IConnection connection, ArraySegment<byte> message)
            {
                if (players.TryGetValue(connection, out INetworkPlayer player))
                {
                    messageHandler.HandleMessage(player, message);
                }
                else
                {
                    // todo remove or replace with assert
                    if (logger.WarnEnabled()) logger.LogWarning($"No player found for [{connection}]");
                }
            }
        }
    }
}
