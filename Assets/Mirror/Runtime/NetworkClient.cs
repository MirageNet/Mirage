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
        /// List of prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.</para>
        /// </summary>
        public List<GameObject> spawnPrefabs = new List<GameObject>();

        readonly Dictionary<uint, NetworkIdentity> spawned = new Dictionary<uint, NetworkIdentity>();

        public readonly NetworkTime Time = new NetworkTime();

        /// <summary>
        /// List of all objects spawned in this client
        /// </summary>
        public Dictionary<uint, NetworkIdentity> Spawned
        {
            get
            {
                // if we are in host mode,  the list of spawned object is the same as the server list
                if (hostServer != null)
                    return hostServer.spawned;
                else
                    return spawned;
            }
        }

        /// <summary>
        /// The host server
        /// </summary>
        NetworkServer hostServer;

        /// <summary>
        /// Transport to use to connect to server
        /// </summary>
        public Transport2 Transport;

        /// <summary>
        /// NetworkClient can connect to local server in host mode too
        /// </summary>
        public bool isLocalClient => hostServer != null;

        /// <summary>
        /// Connect client to a NetworkServer instance.
        /// </summary>
        /// <param name="uri">Address of the server to connect to</param>
        public async Task ConnectAsync(Uri uri)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect: " + uri);

            connectState = ConnectState.Connecting;
            IConnection transportConnection = await Transport.ConnectAsync(uri);

            // setup all the handlers
            connection = new NetworkConnectionToServer(transportConnection);
            RegisterClientHandlers(connection);
            OnConnected();
        }

        internal void ConnectHost(NetworkServer server)
        {
            if (LogFilter.Debug) Debug.Log("Client Connect Host to Server");

            connectState = ConnectState.Connected;

            // create local connection objects and connect them
            var connectionToServer = new ULocalConnectionToServer();
            var connectionToClient = new ULocalConnectionToClient();
            connectionToServer.connectionToClient = connectionToClient;
            connectionToClient.connectionToServer = connectionToServer;

            connection = connectionToServer;
            RegisterHostHandlers(connection);

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
        }

        void OnError(Exception exception)
        {
            Debug.LogException(exception);
        }

        void OnDisconnected()
        {
            connectState = ConnectState.Disconnected;

            ClientScene.HandleClientDisconnect(connection);
        }

        void OnConnected()
        {
            // reset network time stats
            Time.Reset();

            // the handler may want to send messages to the client
            // thus we should set the connected state before calling the handler
            connectState = ConnectState.Connected;
            Time.UpdateClient(this);
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
                hostServer.RemoveLocalConnection();
            }
            else
            {
                if (connection != null)
                {
                    connection.Disconnect();
                }
            }
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
        public void Send<T>(T message, int channelId = Channels.DefaultReliable) where T : IMessageBase
        {
            connection.Send(message, channelId);
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
                    Time.UpdateClient(this);
                }
            }
        }

        internal void RegisterSpawnPrefabs()
        {
            for (int i = 0; i < spawnPrefabs.Count; i++)
            {
                GameObject prefab = spawnPrefabs[i];
                if (prefab != null)
                {
                    ClientScene.RegisterPrefab(prefab);
                }
            }
        }

        internal void RegisterHostHandlers(NetworkConnection connection)
        {
            // host mode client / regular client react to some messages differently.
            // but we still need to add handlers for all of them to avoid
            // 'message id not found' errors.

            connection.RegisterHandler<ObjectDestroyMessage>(ClientScene.OnHostClientObjectDestroy);
            connection.RegisterHandler<ObjectHideMessage>(ClientScene.OnHostClientObjectHide);
            connection.RegisterHandler<NetworkPongMessage>(msg => { }, false);
            connection.RegisterHandler<SpawnMessage>(ClientScene.OnHostClientSpawn);
            connection.RegisterHandler<ObjectSpawnStartedMessage>(msg => { }); // host mode doesn't need spawning
            connection.RegisterHandler<ObjectSpawnFinishedMessage>( msg => { }); // host mode doesn't need spawning
            connection.RegisterHandler<UpdateVarsMessage>(msg => { });
            connection.RegisterHandler<RpcMessage>(ClientScene.OnRPCMessage);
            connection.RegisterHandler<SyncEventMessage>(ClientScene.OnSyncEventMessage);
        }

        internal void RegisterClientHandlers(NetworkConnection connection)
        {
            connection.RegisterHandler<ObjectDestroyMessage>(ClientScene.OnObjectDestroy);
            connection.RegisterHandler<ObjectHideMessage>(ClientScene.OnObjectHide);
            connection.RegisterHandler<NetworkPongMessage>(Time.OnClientPong, false);
            connection.RegisterHandler<SpawnMessage>(ClientScene.OnSpawn);
            connection.RegisterHandler<ObjectSpawnStartedMessage>(ClientScene.OnObjectSpawnStarted);
            connection.RegisterHandler<ObjectSpawnFinishedMessage>(ClientScene.OnObjectSpawnFinished);
            connection.RegisterHandler<UpdateVarsMessage>(ClientScene.OnUpdateVarsMessage);
            connection.RegisterHandler<RpcMessage>(ClientScene.OnRPCMessage);
            connection.RegisterHandler<SyncEventMessage>(ClientScene.OnSyncEventMessage);
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
            // disconnect the client connection.
            // we do NOT call Transport.Shutdown, because someone only called
            // NetworkClient.Shutdown. we can't assume that the server is
            // supposed to be shut down too!
            connection.Disconnect();
        }
    }
}
