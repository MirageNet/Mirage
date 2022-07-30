using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ClientServerSetup<T> : TestBase where T : NetworkBehaviour
    {
        protected ServerInstance<T> _server;
        protected ClientInstance<T> _client;

        // properties to make it easier to use in tests
        protected GameObject serverGo => _server.go;
        protected NetworkServer server => _server.server;
        protected ServerObjectManager serverObjectManager => _server.serverObjectManager;
        protected GameObject serverPlayerGO => _server.character;
        protected NetworkIdentity serverIdentity => _server.identity;
        protected T serverComponent => _server.component;
        protected INetworkPlayer serverPlayer => _server.FirstPlayer;
        protected MessageHandler ServerMessageHandler => server.MessageHandler;


        // properties to make it easier to use in tests
        protected GameObject clientGo => _client.go;
        protected NetworkClient client => _client.client;
        protected ClientObjectManager clientObjectManager => _client.clientObjectManager;
        protected GameObject clientPlayerGO => _client.character;
        protected NetworkIdentity clientIdentity => _client.identity;
        protected T clientComponent => _client.component;
        protected INetworkPlayer clientPlayer => _client.player;


        protected GameObject playerPrefab;

        /// <summary>
        /// network player instance on server that represents the client
        /// <para>NOT the local player</para>
        /// </summary>
        protected MessageHandler ClientMessageHandler => client.MessageHandler;

        /// <summary>
        /// called before Start() after Server/Client GameObject have been setup
        /// </summary>
        public virtual void ExtraSetup() { }
        /// <summary>
        /// Called after test of setup
        /// </summary>
        /// <returns></returns>
        public virtual UniTask LateSetup() => UniTask.CompletedTask;

        protected virtual bool AutoConnectClient => true;
        protected virtual Config ServerConfig => null;
        protected virtual Config ClientConfig => null;



        [UnitySetUp]
        public IEnumerator UnitySetUp() => UniTask.ToCoroutine(async () =>
        {
            Console.WriteLine($"[MirageTest] UnitySetUp class:{TestContext.CurrentContext.Test.ClassName} method:{TestContext.CurrentContext.Test.MethodName} ");

            _server = new ServerInstance<T>(ServerConfig);
            _client = new ClientInstance<T>(ClientConfig, _server.socketFactory);

            ExtraSetup();

            // wait 2 frames for start to be called
            await UniTask.DelayFrame(2);

            // create and register a prefab
            playerPrefab = new GameObject("player (unspawned)", typeof(NetworkIdentity), typeof(T));
            // DontDestroyOnLoad so that "prefab" wont be destroyed by scene loading
            // also means that NetworkScenePostProcess will skip this unspawned object
            Object.DontDestroyOnLoad(playerPrefab);

            var identity = playerPrefab.GetComponent<NetworkIdentity>();
            identity.PrefabHash = Guid.NewGuid().GetHashCode();
            clientObjectManager.RegisterPrefab(identity);

            // wait for client and server to initialize themselves
            await UniTask.Delay(1);

            // start the server
            var started = new UniTaskCompletionSource();
            server.Started.AddListener(() => started.TrySetResult());
            server.StartServer();

            await started.Task;

            if (AutoConnectClient)
            {
                // now start the client
                client.Connect("localhost");

                await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count > 0);

                // create a player object in the server
                _server.SpawnPlayerForFirstClient(playerPrefab);

                // wait for client to spawn it
                await AsyncUtil.WaitUntilWithTimeout(() => client.Player.HasCharacter);

                _client.SetupCharacter();
            }

            await LateSetup();
        });

        public virtual void ExtraTearDown() { }
        public virtual UniTask ExtraTearDownAsync() => UniTask.CompletedTask;

        [UnityTearDown]
        public IEnumerator UnityTearDown() => UniTask.ToCoroutine(async () =>
        {
            // check active, it might have been stopped by tests
            if (client.Active) client.Disconnect();
            if (server.Active) server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !client.Active);
            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            Object.DestroyImmediate(playerPrefab);
            Object.DestroyImmediate(serverGo);
            Object.DestroyImmediate(clientGo);
            Object.DestroyImmediate(serverPlayerGO);
            Object.DestroyImmediate(clientPlayerGO);

            TearDownTestObjects();

            ExtraTearDown();
            await ExtraTearDownAsync();

            Console.WriteLine($"[MirageTest] UnityTearDown class:{TestContext.CurrentContext.Test.ClassName} method:{TestContext.CurrentContext.Test.MethodName}");
        });

    }

    /// <summary>
    /// Instance of Server for <see cref="ClientServerSetup{T}"/>
    /// </summary>
    public class ServerInstance<T>
    {
        public GameObject go;
        public NetworkServer server;
        public ServerObjectManager serverObjectManager;
        public GameObject character;
        public NetworkIdentity identity;
        public T component;
        public INetworkPlayer FirstPlayer;
        /// <summary>
        /// Clients that want to connect to this Instance should use this socket factory
        /// </summary>
        public TestSocketFactory socketFactory;

        public ServerInstance(Config config)
        {
            go = new GameObject("server", typeof(ServerObjectManager), typeof(NetworkServer));
            server = go.GetComponent<NetworkServer>();
            if (config != null) server.PeerConfig = config;
            socketFactory = go.AddComponent<TestSocketFactory>();
            server.SocketFactory = socketFactory;

            serverObjectManager = go.GetComponent<ServerObjectManager>();
            serverObjectManager.Server = server;
        }

        public void SpawnPlayerForFirstClient(GameObject prefab)
        {
            FirstPlayer = server.Players.First();

            character = Object.Instantiate(prefab);
            character.name = "player (server)";
            identity = character.GetComponent<NetworkIdentity>();
            component = character.GetComponent<T>();
            serverObjectManager.AddCharacter(FirstPlayer, character);
        }
    }

    /// <summary>
    /// Instance of Client for <see cref="ClientServerSetup{T}"/>
    /// </summary>
    public class ClientInstance<T>
    {
        public GameObject go;
        public NetworkClient client;
        public ClientObjectManager clientObjectManager;
        public GameObject character;
        public NetworkIdentity identity;
        public T component;
        public INetworkPlayer player;

        public ClientInstance(Config config, TestSocketFactory socketFactory)
        {
            go = new GameObject("client", typeof(ClientObjectManager), typeof(NetworkClient));
            client = go.GetComponent<NetworkClient>();
            if (config != null) client.PeerConfig = config;
            client.SocketFactory = socketFactory;

            clientObjectManager = go.GetComponent<ClientObjectManager>();
            clientObjectManager.Client = client;
        }

        public void SetupCharacter()
        {
            // get the connections so that we can spawn players
            player = client.Player;
            identity = player.Identity;
            character = identity.gameObject;
            character.name = "player (client)";
            component = character.GetComponent<T>();
        }
    }
}
