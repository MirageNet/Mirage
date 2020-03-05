using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror
{
    public enum ConnectState
    {
        None,
        Connecting,
        Connected,
        Disconnected
    }

    /// <summary>
    /// This is a network client class used by the networking system. It contains a NetworkConnection that is used to connect to a network server.
    /// <para>The <see cref="NetworkClient">NetworkClient</see> handle connection state, messages handlers, and connection configuration. There can be many <see cref="NetworkClient">NetworkClient</see> instances in a process at a time, but only one that is connected to a game server (<see cref="NetworkServer">NetworkServer</see>) that uses spawned objects.</para>
    /// <para><see cref="NetworkClient">NetworkClient</see> has an internal update function where it handles events from the transport layer. This includes asynchronous connect events, disconnect events and incoming data from a server.</para>
    /// <para>The <see cref="NetworkManager">NetworkManager</see> has a NetworkClient instance that it uses for games that it starts, but the NetworkClient may be used by itself.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class NetworkClient : MonoBehaviour
    {
        // Deprecated 03/25/2019
        /// <summary>
        /// The registered network message handlers.
        /// </summary>
        readonly Dictionary<int, NetworkMessageDelegate> handlers = new Dictionary<int, NetworkMessageDelegate>();

        /// <summary>
        /// The NetworkConnection object this client is using.
        /// </summary>
        public NetworkConnection connection { get; internal set; }

        internal ConnectState connectState = ConnectState.None;

        /// <summary>
        /// active is true while a client is connecting/connected
        /// (= while the network is active)
        /// </summary>
        public bool active => connectState == ConnectState.Connecting || connectState == ConnectState.Connected;

        /// <summary>
        /// This gives the current connection status of the client.
        /// </summary>
        public bool isConnected => connectState == ConnectState.Connected;

        /// <summary>
        /// The host server
        /// </summary>
        private NetworkServer hostServer;

        /// <summary>
        /// NetworkClient can connect to local server in host mode too
        /// </summary>
        public bool isLocalClient => hostServer != null;

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="address"></param>
        public async Task ConnectAsync(string serverIp)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect: " + serverIp);

            RegisterSystemHandlers(false);
            Transport.activeTransport.enabled = true;
            InitializeTransportHandlers();

            connectState = ConnectState.Connecting;
            await Transport.activeTransport.ClientConnectAsync(serverIp);

            // setup all the handlers
            connection = new NetworkConnectionToServer();
            connection.SetHandlers(handlers);
            OnConnected();
        }

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="uri">Address of the server to connect to</param>
        public async Task ConnectAsync(Uri uri)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect: " + uri);

            RegisterSystemHandlers(false);
            Transport.activeTransport.enabled = true;
            InitializeTransportHandlers();

            connectState = ConnectState.Connecting;
            await Transport.activeTransport.ClientConnectAsync(uri);

            // setup all the handlers
            connection = new NetworkConnectionToServer();
            connection.SetHandlers(handlers);
            OnConnected();
        }

        internal void ConnectHost(NetworkServer server)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect Host to Server");

            RegisterSystemHandlers(true);

            connectState = ConnectState.Connected;

            // create local connection objects and connect them
            var connectionToServer = new ULocalConnectionToServer();
            var connectionToClient = new ULocalConnectionToClient();
            connectionToServer.connectionToClient = connectionToClient;
            connectionToClient.connectionToServer = connectionToServer;

            connection = connectionToServer;
            connection.SetHandlers(handlers);

            // create server connection to local client
            server.SetLocalConnection(this, connectionToClient);
            hostServer = server;
        }

        /// <summary>
        /// connect host mode
        /// </summary>
        internal void ConnectLocalServer(NetworkServer server)
        {
            server.OnConnected(server.localConnection);
            server.localConnection.Send(new ConnectMessage());
        }

        void InitializeTransportHandlers()
        {
            Transport.activeTransport.OnClientDataReceived.AddListener(OnDataReceived);
            Transport.activeTransport.OnClientDisconnected.AddListener(OnDisconnected);
            Transport.activeTransport.OnClientError.AddListener(OnError);
        }

        void OnError(Exception exception)
        {
            Debug.LogException(exception);
        }

        void OnDisconnected()
        {
            connectState = ConnectState.Disconnected;

            ClientScene.HandleClientDisconnect(connection);

            connection?.InvokeHandler(new DisconnectMessage(), -1);
        }

        internal void OnDataReceived(ArraySegment<byte> data, int channelId)
        {
            if (connection != null)
            {
                connection.TransportReceive(data, channelId);
            }
            else Debug.LogError("Skipped Data message handling because connection is null.");
        }

        void OnConnected()
        {
            // reset network time stats
            NetworkTime.Reset();

            // the handler may want to send messages to the client
            // thus we should set the connected state before calling the handler
            connectState = ConnectState.Connected;
            NetworkTime.UpdateClient(this);
            connection.InvokeHandler(new ConnectMessage(), -1);
        }

        /// <summary>
        /// Disconnect from server.
        /// <para>The disconnect message will be invoked.</para>
        /// </summary>
        public void Disconnect()
        {
            connectState = ConnectState.Disconnected;
            ClientScene.HandleClientDisconnect(connection);

            // local or remote connection?
            if (isLocalClient)
            {
                if (isConnected)
                {
                    hostServer.localConnection.Send(new DisconnectMessage());
                }
                hostServer.RemoveLocalConnection();
            }
            else
            {
                if (connection != null)
                {
                    connection.Disconnect();
                    connection.Dispose();
                    connection = null;
                    RemoveTransportHandlers();
                }
            }
        }

        void RemoveTransportHandlers()
        {
            // so that we don't register them more than once
            Transport.activeTransport.OnClientDataReceived.RemoveListener(OnDataReceived);
            Transport.activeTransport.OnClientDisconnected.RemoveListener(OnDisconnected);
            Transport.activeTransport.OnClientError.RemoveListener(OnError);
        }

        // Deprecated 03/03/2019
        /// <summary>
        /// This sends a network message with a message Id to the server. This message is sent on channel zero, which by default is the reliable channel.
        /// <para>The message must be an instance of a class derived from MessageBase.</para>
        /// <para>The message id passed to Send() is used to identify the handler function to invoke on the server when the message is received.</para>
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="message"></param>
        /// <param name="channelId"></param>
        /// <returns>True if message was sent.</returns>
        public bool Send<T>(T message, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            if (connection != null)
            {
                if (connectState != ConnectState.Connected)
                {
                    Debug.LogError("NetworkClient Send when not connected to a server");
                    return false;
                }
                return connection.Send(message, channelId);
            }
            Debug.LogError("NetworkClient Send with no connection");
            return false;
        }

        internal void Update()
        {
            // local connection?
            if (connection is ULocalConnectionToServer localConnection)
            {
                localConnection.Update();
            }
            // remote connection?
            else
            {
                // only update things while connected
                if (active && connectState == ConnectState.Connected)
                {
                    NetworkTime.UpdateClient(this);
                }
            }
        }

        internal void RegisterSystemHandlers(bool hostMode)
        {
            // host mode client / regular client react to some messages differently.
            // but we still need to add handlers for all of them to avoid
            // 'message id not found' errors.
            if (hostMode)
            {
                RegisterHandler<ObjectDestroyMessage>(ClientScene.OnHostClientObjectDestroy);
                RegisterHandler<ObjectHideMessage>(ClientScene.OnHostClientObjectHide);
                RegisterHandler<NetworkPongMessage>((conn, msg) => { }, false);
                RegisterHandler<SpawnMessage>(ClientScene.OnHostClientSpawn);
                RegisterHandler<ObjectSpawnStartedMessage>((conn, msg) => { }); // host mode doesn't need spawning
                RegisterHandler<ObjectSpawnFinishedMessage>((conn, msg) => { }); // host mode doesn't need spawning
                RegisterHandler<UpdateVarsMessage>((conn, msg) => { });
            }
            else
            {
                RegisterHandler<ObjectDestroyMessage>(ClientScene.OnObjectDestroy);
                RegisterHandler<ObjectHideMessage>(ClientScene.OnObjectHide);
                RegisterHandler<NetworkPongMessage>(NetworkTime.OnClientPong, false);
                RegisterHandler<SpawnMessage>(ClientScene.OnSpawn);
                RegisterHandler<ObjectSpawnStartedMessage>(ClientScene.OnObjectSpawnStarted);
                RegisterHandler<ObjectSpawnFinishedMessage>(ClientScene.OnObjectSpawnFinished);
                RegisterHandler<UpdateVarsMessage>(ClientScene.OnUpdateVarsMessage);
            }
            RegisterHandler<RpcMessage>(ClientScene.OnRPCMessage);
            RegisterHandler<SyncEventMessage>(ClientScene.OnSyncEventMessage);
        }

        // Deprecated 03/03/2019
        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="handler"></param>
        /// <param name="requireAuthentication">true if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<NetworkConnectionToServer, T> handler, bool requireAuthentication = true) where T : IMessageBase, new()
        {
            int msgType = MessagePacker.GetId<T>();
            if (handlers.ContainsKey(msgType))
            {
                if (LogFilter.Debug) Debug.Log("NetworkClient.RegisterHandler replacing " + handler + " - " + msgType);
            }
            handlers[msgType] = MessagePacker.MessageHandler(handler, requireAuthentication);
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        /// <param name="handler"></param>
        /// <param name="requireAuthentication">true if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : IMessageBase, new()
        {
            RegisterHandler((NetworkConnectionToServer _, T value) => { handler(value); }, requireAuthentication);
        }

        /// <summary>
        /// Unregisters a network message handler.
        /// </summary>
        /// <typeparam name="T">The message type to unregister.</typeparam>
        public void UnregisterHandler<T>() where T : IMessageBase
        {
            // use int to minimize collisions
            int msgType = MessagePacker.GetId<T>();
            handlers.Remove(msgType);
        }

        /// <summary>
        /// Shut down a client.
        /// <para>This should be done when a client is no longer going to be used.</para>
        /// </summary>
        public void Shutdown()
        {
            if (LogFilter.Debug) Debug.Log("Shutting down client.");
            ClientScene.Shutdown();
            connectState = ConnectState.None;
            handlers.Clear();
            // disconnect the client connection.
            // we do NOT call Transport.Shutdown, because someone only called
            // NetworkClient.Shutdown. we can't assume that the server is
            // supposed to be shut down too!
            Transport.activeTransport.ClientDisconnect();
        }
    }
}
