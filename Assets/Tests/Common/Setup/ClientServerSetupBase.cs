using System;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Mirage.Tests
{
    /// <summary>
    /// base class with setup methods so that it can be called from runtime or editor tests
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientServerSetupBase<T> : TestBase where T : NetworkBehaviour
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

        public async UniTask ClientServerSetUp()
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
        }

        public virtual void ExtraTearDown() { }
        public virtual UniTask ExtraTearDownAsync() => UniTask.CompletedTask;

        public async UniTask ClientServerTearDown()
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
        }
    }
}
