using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.Host
{

    public class HostSetup<T> : TestBase where T : NetworkBehaviour
    {
        protected GameObject networkManagerGo;
        protected NetworkManager manager;
        protected NetworkServer server;
        protected NetworkClient client;
        protected ServerObjectManager serverObjectManager;
        protected ClientObjectManager clientObjectManager;

        protected GameObject playerGO;
        protected NetworkIdentity playerIdentity;
        protected T playerComponent;

        protected MessageHandler ClientMessageHandler => client.MessageHandler;
        protected MessageHandler ServerMessageHandler => server.MessageHandler;

        protected virtual bool AutoStartServer => true;
        protected virtual Config ServerConfig => null;
        protected virtual Config ClientConfig => null;

        /// <summary>
        /// called before Start() after Server/Client GameObject have been setup
        /// </summary>
        public virtual void ExtraSetup() { }
        /// <summary>
        /// Called after test of setup
        /// </summary>
        /// <returns></returns>
        public virtual UniTask LateSetup() => UniTask.CompletedTask;

        [UnitySetUp]
        public IEnumerator UnitySetUp() => UniTask.ToCoroutine(async () =>
        {
            Console.WriteLine($"[MirageTest] UnitySetUp class:{TestContext.CurrentContext.Test.ClassName} method:{TestContext.CurrentContext.Test.MethodName}");

            networkManagerGo = new GameObject();

            networkManagerGo.AddComponent<TestSocketFactory>();
            serverObjectManager = networkManagerGo.AddComponent<ServerObjectManager>();
            clientObjectManager = networkManagerGo.AddComponent<ClientObjectManager>();
            manager = networkManagerGo.AddComponent<NetworkManager>();
            server = networkManagerGo.AddComponent<NetworkServer>();
            client = networkManagerGo.AddComponent<NetworkClient>();
            manager.Client = networkManagerGo.GetComponent<NetworkClient>();
            manager.Server = networkManagerGo.GetComponent<NetworkServer>();

            if (ServerConfig != null) server.PeerConfig = ServerConfig;
            if (ClientConfig != null) client.PeerConfig = ClientConfig;

            serverObjectManager.Server = server;
            clientObjectManager.Client = client;

            ExtraSetup();

            // wait for all Start() methods to get invoked
            await UniTask.DelayFrame(2);

            if (AutoStartServer)
            {
                await StartHost();

                playerGO = new GameObject("playerGO", typeof(Rigidbody));
                playerIdentity = playerGO.AddComponent<NetworkIdentity>();
                playerIdentity.PrefabHash = Guid.NewGuid().GetHashCode();
                playerComponent = playerGO.AddComponent<T>();

                serverObjectManager.AddCharacter(server.LocalPlayer, playerGO);

                // wait for client to spawn it
                await AsyncUtil.WaitUntilWithTimeout(() => client.Player.HasCharacter);
            }

            await LateSetup();
        });

        protected async UniTask StartHost()
        {
            var completionSource = new UniTaskCompletionSource();

            void Started()
            {
                completionSource.TrySetResult();
            }

            server.Started.AddListener(Started);
            // now start the host
            manager.Server.StartServer(client);

            await completionSource.Task;
        }

        public virtual void ExtraTearDown() { }
        public virtual UniTask ExtraTearDownAsync() => UniTask.CompletedTask;

        [UnityTearDown]
        public IEnumerator UnityTearDown() => UniTask.ToCoroutine(async () =>
        {
            Object.Destroy(playerGO);

            // check active, it might have been stopped by tests
            if (server.Active) server.Stop();

            await UniTask.Delay(1);
            Object.Destroy(networkManagerGo);

            TearDownTestObjects();

            ExtraTearDown();
            await ExtraTearDownAsync();

            Console.WriteLine($"[MirageTest] UnityTearDown class:{TestContext.CurrentContext.Test.ClassName} method:{TestContext.CurrentContext.Test.MethodName}");
        });

        public void DoUpdate(int updateCount = 1)
        {
            for (var i = 0; i < updateCount; i++)
            {
                server.Update();
                client.Update();
            }
        }
    }
}
