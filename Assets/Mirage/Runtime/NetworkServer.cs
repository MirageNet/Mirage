using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Mirage.Authentication;
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
    [HelpURL("https://miragenet.github.io/Mirage/docs/reference/Mirage/NetworkServer")]
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
        [Tooltip("Should the message handler rethrow the exception after logging. This should only be used when deubgging as it may stop other Mirage functions from running after messages handling")]
        public bool RethrowException = false;

        [Tooltip("If true will set Application.runInBackground")]
        public bool RunInBackground = true;

        [Tooltip("If disabled the server will not create a Network Peer to listen. This can be used to run server single player mode")]
        public bool Listening = true;

        [Tooltip("Creates Socket for Peer to use")]
        public SocketFactory SocketFactory;

        public ServerObjectManager ObjectManager;

        private Peer _peer;
        public PoolMetrics? PeerPoolMetrics => _peer?.PoolMetrics;

        [Tooltip("Authentication component attached to this object")]
        public AuthenticatorSettings Authenticator;

        [Header("Events")]
        [SerializeField] private AddLateEventUnity _started = new AddLateEventUnity();
        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// </summary>
        public IAddLateEventUnity Started => _started;

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

        [SerializeField] private AddLateEventUnity _stopped = new AddLateEventUnity();
        public IAddLateEventUnity Stopped => _stopped;

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        [SerializeField] private AddLateEventUnity _onStartHost = new AddLateEventUnity();
        public IAddLateEventUnity OnStartHost => _onStartHost;

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        [SerializeField] private AddLateEventUnity _onStopHost = new AddLateEventUnity();
        public IAddLateEventUnity OnStopHost => _onStopHost;

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
        [System.Obsolete("use IsHost instead")]
        public bool LocalClientActive => IsHost;
        /// <summary>
        /// True if there is a local client connected to this server (host mode)
        /// </summary>
        public bool IsHost => LocalClient != null && LocalClient.Active;

        /// <summary>
        /// All players on server (including unauthenticated players)
        /// </summary>
        public IReadOnlyCollection<INetworkPlayer> AllPlayers => _connections.Values;
        [Obsolete("Use AllPlayers or AuthenticatedPlayers instead")]
        public IReadOnlyCollection<INetworkPlayer> Players => _connections.Values;

        /// <summary>
        /// List of players that have Authenticated with server
        /// </summary>
        public IReadOnlyList<INetworkPlayer> AuthenticatedPlayers => _authenticatedPlayers;

        private readonly Dictionary<IConnection, INetworkPlayer> _connections = new Dictionary<IConnection, INetworkPlayer>();
        private readonly List<INetworkPlayer> _authenticatedPlayers = new List<INetworkPlayer>();

        /// <summary>
        /// <para>Checks if the server has been started.</para>
        /// <para>This will be true after NetworkServer.Listen() has been called.</para>
        /// </summary>
        public bool Active { get; private set; }

        public NetworkWorld World { get; private set; }
        // todo move syncVarsender, it doesn't need to be a public fields on network server any more
        public SyncVarSender SyncVarSender { get; private set; }

        private SyncVarReceiver _syncVarReceiver;
        public MessageHandler MessageHandler { get; private set; }

        private Action<INetworkPlayer, AuthenticationResult> _authFallCallback;

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
            _authenticatedPlayers.Clear();
            LocalClient = null;
            LocalPlayer = null;

            _stopped?.Invoke();
            Active = false;

            _started.Reset();
            _onStartHost.Reset();
            _onStopHost.Reset();
            _stopped.Reset();

            World = null;
            SyncVarSender = null;

            if (_peer != null)
            {
                //remove handlers first to stop loop
                _peer.OnConnected -= Peer_OnConnected;
                _peer.OnDisconnected -= Peer_OnDisconnected;
                _peer.Close();
                _peer = null;
            }

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
            if (Listening)
                ThrowIfSocketIsMissing();

            Application.quitting += Stop;
            if (logger.LogEnabled()) logger.Log($"NetworkServer created, Mirage version: {Version.Current}");

            logger.Assert(_authenticatedPlayers.Count == 0, "Player should have been reset since previous session");
            logger.Assert(_connections.Count == 0, "Connections should have been reset since previous session");

            World = new NetworkWorld();
            SyncVarSender = new SyncVarSender();

            LocalClient = localClient;
            MessageHandler = new MessageHandler(World, DisconnectOnException, RethrowException);
            MessageHandler.RegisterHandler<NetworkPingMessage>(World.Time.OnServerPing, allowUnauthenticated: true);

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

            // Are we listening for incoming connections?
            // If yes, set up a socket for incoming connections (we're a multiplayer game).
            // If not, that's okay. Some games use a non-listening server for their single player game mode (Battlefield, Call of Duty...)
            if (Listening)
            {
                var maxPacketSize = SocketFactory.MaxPacketSize;
                NetworkWriterPool.Configure(maxPacketSize);

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

            if (Authenticator != null)
                Authenticator.Setup(this);

            Active = true;
            // make sure to call ServerObjectManager start before started event
            // this is too stop any race conditions where other scripts add their started event before SOM is setup
            if (ObjectManager != null)
            {
                ObjectManager.ServerStarted(this);
                // if no hostClient, then  spawn objects right away
                if (LocalClient == null)
                    ObjectManager.SpawnOrActivate();
            }
            _started?.Invoke();

            if (LocalClient != null)
            {
                localClient.ConnectHost(this, dataHandler);

                // onStartHost needs to be called after the client is active
                _onStartHost?.Invoke();

                // spawn scene objects in starting scene AFTER host client has activated,
                // otherwise IsClient will be false for objects in starting scene
                if (ObjectManager != null)
                    ObjectManager.SpawnOrActivate();

                Connected?.Invoke(LocalPlayer);

                if (logger.LogEnabled()) logger.Log("NetworkServer StartHost");
                Authenticate(LocalPlayer);
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
            var player = new NetworkPlayer(conn, false);
            if (logger.LogEnabled()) logger.Log($"Server new player {player}");

            // add connection
            _connections[player.Connection] = player;

            // let everyone know we just accepted a connection
            Connected?.Invoke(player);

            Authenticate(player);
        }

        private void Authenticate(INetworkPlayer player)
        {
            // authenticate player
            if (Authenticator != null)
                AuthenticateAsync(player).Forget();
            else
                AuthenticationSuccess(player, AuthenticationResult.CreateSuccess("No Authenticators"));
        }

        private async UniTaskVoid AuthenticateAsync(INetworkPlayer player)
        {
            var result = await Authenticator.ServerAuthenticate(player);

            // process results
            if (result.Success)
            {
                AuthenticationSuccess(player, result);
            }
            else
            {
                if (_authFallCallback != null)
                {
                    if (logger.LogEnabled()) logger.Log($"Calling user auth failed callback");
                    _authFallCallback.Invoke(player, result);
                }
                else
                {
                    if (logger.LogEnabled()) logger.Log($"Default auth failed, disconnecting player");
                    player.Disconnect();
                }
            }
        }

        public void SetAuthenticationFailedCallback(Action<INetworkPlayer, AuthenticationResult> callback)
        {
            if (_authFallCallback != null && callback != null && logger.WarnEnabled())
                logger.LogWarning($"Replacing old callback. Only 1 auth failed callback can be used at once");

            _authFallCallback = callback;
        }

        private void AuthenticationSuccess(INetworkPlayer player, AuthenticationResult result)
        {
            player.SetAuthentication(new PlayerAuthentication(result.Authenticator, result.Data));

            // send message to let client know
            //     we want to send this even if host, or no Authenticators
            //     this makes host logic a lot easier,
            //     because we need to call SetAuthentication on both server/client before Authenticated
            player.Send(new AuthSuccessMessage { AuthenticatorName = result.Authenticator?.AuthenticatorName });

            // add connection
            _authenticatedPlayers.Add(player);
            Authenticated?.Invoke(player);
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
        /// This removes an external connection.
        /// </summary>
        /// <param name="connectionId">The id of the connection to remove.</param>
        private void RemoveConnection(INetworkPlayer player)
        {
            _connections.Remove(player.Connection);
            _authenticatedPlayers.Remove(player);
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

            var player = new NetworkPlayer(connection, true);
            LocalPlayer = player;
            LocalClient = client;

            if (logger.LogEnabled()) logger.Log($"Server accepted local client connection: {player}");

            _connections[player.Connection] = player;

            if (Authenticator != null)
                // we need to add host player to auth early, so that Client.Connected, can be used to send auth message
                // if we want for server to add it then we will be too late 
                Authenticator.PreAddHostPlayer(player);
        }

        [Obsolete("Use SendToAll(msg, authenticatedOnly, excludeLocalPlayer, channelId) instead")]
        public void SendToAll<T>(T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable) => SendToAll(msg, authenticatedOnly: false, excludeLocalPlayer, channelId);
        
        public void SendToAll<T>(T msg, bool authenticatedOnly, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
        {
            if (authenticatedOnly)
            {
                SendToMany(_authenticatedPlayers, msg, excludeLocalPlayer);
            }
            else
            {
                var enumerator = _connections.Values.GetEnumerator();
                SendToMany(enumerator, msg, excludeLocalPlayer, channelId);
            }
        }

        public void SendToMany<T>(IReadOnlyList<INetworkPlayer> players, T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
        {
            if (excludeLocalPlayer)
            {
                using (var list = AutoPool<List<INetworkPlayer>>.Take())
                {
                    ListHelper.AddToList(list, players, LocalPlayer);
                    NetworkServer.SendToMany(list, msg, channelId);
                }
            }
            else
            {
                // we are not removing any objects from the list, so we can skip the AddToList
                NetworkServer.SendToMany(players, msg, channelId);
            }
        }
        /// <summary>
        /// Warning: this will allocate, Use <see cref="SendToMany{T}(IReadOnlyList{INetworkPlayer}, T, bool, Channel)"/> or <see cref="SendToMany{T, TEnumerator}(TEnumerator, T, bool, Channel)"/> instead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="players"></param>
        /// <param name="msg"></param>
        /// <param name="excludeLocalPlayer"></param>
        /// <param name="channelId"></param>
        public void SendToMany<T>(IEnumerable<INetworkPlayer> players, T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
        {
            using (var list = AutoPool<List<INetworkPlayer>>.Take())
            {
                ListHelper.AddToList(list, players, excludeLocalPlayer ? LocalPlayer : null);
                NetworkServer.SendToMany(list, msg, channelId);
            }
        }
        /// <summary>
        /// use to avoid allocation of IEnumerator
        /// </summary>
        public void SendToMany<T, TEnumerator>(TEnumerator playerEnumerator, T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
            where TEnumerator : struct, IEnumerator<INetworkPlayer>
        {
            using (var list = AutoPool<List<INetworkPlayer>>.Take())
            {
                ListHelper.AddToList(list, playerEnumerator, excludeLocalPlayer ? LocalPlayer : null);
                NetworkServer.SendToMany(list, msg, channelId);
            }
        }

        public void SendToObservers<T>(NetworkIdentity identity, T msg, bool excludeLocalPlayer, bool excludeOwner, Channel channelId = Channel.Reliable)
        {
            var observers = identity.observers;
            if (observers.Count == 0)
                return;

            using (var list = AutoPool<List<INetworkPlayer>>.Take())
            {
                var enumerator = observers.GetEnumerator();
                ListHelper.AddToList(list, enumerator, excludeLocalPlayer ? LocalPlayer : null, excludeOwner ? identity.Owner : null);
                NetworkServer.SendToMany(list, msg, channelId);
            }
        }

        /// <summary>
        /// Sends to list of players.
        /// <para>All other SendTo... functions call this, it dooes not do any extra checks, just serializes message if not empty, then sends it</para>
        /// </summary>
        // need explicity List function here, so that implicit casts to List from wrapper works
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendToMany<T>(List<INetworkPlayer> players, T msg, Channel channelId = Channel.Reliable)
            => SendToMany((IReadOnlyList<INetworkPlayer>)players, msg, channelId);

        /// <summary>
        /// Sends to list of players.
        /// <para>All other SendTo... functions call this, it dooes not do any extra checks, just serializes message if not empty, then sends it</para>
        /// </summary>
        public static void SendToMany<T>(IReadOnlyList<INetworkPlayer> players, T msg, Channel channelId = Channel.Reliable)
        {
            // avoid serializing when list is empty
            if (players.Count == 0)
                return;

            using (var writer = NetworkWriterPool.GetWriter())
            {
                if (logger.LogEnabled()) logger.Log($"Sending {typeof(T)} to {players.Count} players, channel:{channelId}");

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
            player.RemoveAllVisibleObjects();

            if (player == LocalPlayer)
                LocalPlayer = null;
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

    public static class NetworkExtensions
    {
        /// <summary>
        /// Send a message to all the remote observers
        /// </summary>
        /// <typeparam name="T">The message type to dispatch.</typeparam>
        /// <param name="msg">The message to deliver to clients.</param>
        /// <param name="includeOwner">Should the owner should receive this message too?</param>
        /// <param name="channelId">The transport channel that should be used to deliver the message. Default is the Reliable channel.</param>
        internal static void SendToRemoteObservers<T>(this NetworkIdentity identity, T msg, bool includeOwner = true, Channel channelId = Channel.Reliable)
        {
            identity.Server.SendToObservers(identity, msg, excludeLocalPlayer: true, excludeOwner: !includeOwner, channelId: channelId);
        }
    }
}
