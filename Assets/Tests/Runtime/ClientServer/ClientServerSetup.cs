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
        protected GameObject serverGo;
        protected NetworkServer server;
        protected ServerObjectManager serverObjectManager;
        protected GameObject serverPlayerGO;
        protected NetworkIdentity serverIdentity;
        protected T serverComponent;

        protected GameObject clientGo;
        protected NetworkClient client;
        protected ClientObjectManager clientObjectManager;
        protected GameObject clientPlayerGO;
        protected NetworkIdentity clientIdentity;
        protected T clientComponent;

        protected GameObject playerPrefab;

        protected TestSocketFactory socketFactory;
        protected INetworkPlayer clientPlayer;
        /// <summary>
        /// network player instance on server that represents the client
        /// <para>NOT the local player</para>
        /// </summary>
        protected INetworkPlayer serverPlayer;
        protected MessageHandler ClientMessageHandler => client.MessageHandler;
        protected MessageHandler ServerMessageHandler => server.MessageHandler;

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

            serverGo = new GameObject("server", typeof(ServerObjectManager), typeof(NetworkServer));
            clientGo = new GameObject("client", typeof(ClientObjectManager), typeof(NetworkClient));
            socketFactory = serverGo.AddComponent<TestSocketFactory>();

            server = serverGo.GetComponent<NetworkServer>();
            client = clientGo.GetComponent<NetworkClient>();

            if (ServerConfig != null) server.PeerConfig = ServerConfig;
            if (ClientConfig != null) client.PeerConfig = ClientConfig;

            server.SocketFactory = socketFactory;
            client.SocketFactory = socketFactory;

            serverObjectManager = serverGo.GetComponent<ServerObjectManager>();
            serverObjectManager.Server = server;

            clientObjectManager = clientGo.GetComponent<ClientObjectManager>();
            clientObjectManager.Client = client;

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

                // get the connections so that we can spawn players
                serverPlayer = server.Players.First();
                clientPlayer = client.Player;

                // create a player object in the server
                serverPlayerGO = Object.Instantiate(playerPrefab);
                serverPlayerGO.name = "player (server)";
                serverIdentity = serverPlayerGO.GetComponent<NetworkIdentity>();
                serverComponent = serverPlayerGO.GetComponent<T>();
                serverObjectManager.AddCharacter(serverPlayer, serverPlayerGO);

                // wait for client to spawn it
                await AsyncUtil.WaitUntilWithTimeout(() => clientPlayer.HasCharacter);

                clientIdentity = clientPlayer.Identity;
                clientPlayerGO = clientIdentity.gameObject;
                clientPlayerGO.name = "player (client)";
                clientComponent = clientPlayerGO.GetComponent<T>();
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
}
