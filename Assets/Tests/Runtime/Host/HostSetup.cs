using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Host
{
    // set's up a host
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

        public virtual void ExtraSetup() { }

        [UnitySetUp]
        public IEnumerator SetupHost() => UniTask.ToCoroutine(async () =>
        {
            networkManagerGo = new GameObject();
            networkManagerGo.AddComponent<MockTransport>();
            sceneManager = networkManagerGo.AddComponent<NetworkSceneManager>();
            serverObjectManager = networkManagerGo.AddComponent<ServerObjectManager>();
            clientObjectManager = networkManagerGo.AddComponent<ClientObjectManager>();
            manager = networkManagerGo.AddComponent<NetworkManager>();
            manager.Client = networkManagerGo.GetComponent<NetworkClient>();
            manager.Server = networkManagerGo.GetComponent<NetworkServer>();
            server = manager.Server;
            client = manager.Client;
            sceneManager.Client = client;
            sceneManager.Server = server;
            serverObjectManager.Server = server;
            serverObjectManager.NetworkSceneManager = sceneManager;
            clientObjectManager.Client = client;
            clientObjectManager.NetworkSceneManager = sceneManager;

            ExtraSetup();

            // wait for all Start() methods to get invoked
            await UniTask.DelayFrame(1);

            await StartHost();

            playerGO = new GameObject("playerGO", typeof(Rigidbody));
            identity = playerGO.AddComponent<NetworkIdentity>();
            component = playerGO.AddComponent<T>();

            serverObjectManager.AddPlayerForConnection(server.LocalConnection, playerGO);

            // wait for client to spawn it
            await AsyncUtil.WaitUntilWithTimeout(() => client.Connection.Identity != null);
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
            manager.Server.StartHost(client).Forget();

            await completionSource.Task;
            server.Started.RemoveListener(Started);
        }

        public virtual void ExtraTearDown() { }

        [UnityTearDown]
        public IEnumerator ShutdownHost() => UniTask.ToCoroutine(async () =>
        {
            Object.Destroy(playerGO);
            manager.Server.StopHost();

            await UniTask.Delay(1);
            Object.Destroy(networkManagerGo);

            ExtraTearDown();
        });

        #endregion
    }
}
