using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class HostSetup<T> where T : NetworkBehaviour
    {

        #region Setup
        protected GameObject networkManagerGo;
        protected NetworkManager manager;
        protected NetworkServer server;
        protected NetworkClient client;
        protected NetworkSceneManager sceneManager;
        protected ServerObjectManager serverObjectManager;
        protected ClientObjectManager clientObjectManager;

        protected GameObject playerGO;
        protected NetworkIdentity identity;
        protected T component;

        protected MessageHandler ClientMessageHandler => client.MessageHandler;
        protected MessageHandler ServerMessageHandler => server.MessageHandler;

        protected virtual bool AutoStartServer => true;
        protected virtual Config ServerConfig => null;
        protected virtual Config ClientConfig => null;

        public virtual void ExtraSetup() { }

        [UnitySetUp]
        public IEnumerator SetupHost() => UniTask.ToCoroutine(async () =>
        {
            networkManagerGo = new GameObject();
            // set gameobject name to test name (helps with debugging)
            networkManagerGo.name = TestContext.CurrentContext.Test.MethodName;

            networkManagerGo.AddComponent<TestSocketFactory>();
            sceneManager = networkManagerGo.AddComponent<NetworkSceneManager>();
            serverObjectManager = networkManagerGo.AddComponent<ServerObjectManager>();
            clientObjectManager = networkManagerGo.AddComponent<ClientObjectManager>();
            manager = networkManagerGo.AddComponent<NetworkManager>();
            manager.Client = networkManagerGo.GetComponent<NetworkClient>();
            manager.Server = networkManagerGo.GetComponent<NetworkServer>();
            server = manager.Server;
            client = manager.Client;

            if (ServerConfig != null) server.PeerConfig = ServerConfig;
            if (ClientConfig != null) client.PeerConfig = ClientConfig;

            sceneManager.Client = client;
            sceneManager.Server = server;
            serverObjectManager.Server = server;
            serverObjectManager.NetworkSceneManager = sceneManager;
            clientObjectManager.Client = client;
            clientObjectManager.NetworkSceneManager = sceneManager;

            ExtraSetup();

            // wait for all Start() methods to get invoked
            await UniTask.DelayFrame(1);

            if (AutoStartServer)
            {
                await StartHost();

                playerGO = new GameObject("playerGO", typeof(Rigidbody));
                identity = playerGO.AddComponent<NetworkIdentity>();
                component = playerGO.AddComponent<T>();

                serverObjectManager.AddCharacter(server.LocalPlayer, playerGO);

                // wait for client to spawn it
                await AsyncUtil.WaitUntilWithTimeout(() => client.Player.HasCharacter);
            }
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

        [UnityTearDown]
        public IEnumerator ShutdownHost() => UniTask.ToCoroutine(async () =>
        {
            Object.Destroy(playerGO);

            // check active, it might have been stopped by tests
            if (server.Active) server.Stop();

            await UniTask.Delay(1);
            Object.Destroy(networkManagerGo);

            ExtraTearDown();
        });

        public void DoUpdate(int updateCount = 1)
        {
            for (int i = 0; i < updateCount; i++)
            {
                server.Update();
                client.Update();
            }
        }

        #endregion
    }
}
