using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using static Mirror.Tests.AsyncUtil;

namespace Mirror.Tests
{
    // set's up a host
    public class HostSetup<T> where T : NetworkBehaviour
    {

        #region Setup
        protected GameObject networkManagerGo;
        protected NetworkHost manager;
        protected NetworkServer server;
        protected NetworkClient client;
        protected NetworkSceneManager sceneManager;

        protected GameObject playerGO;
        protected NetworkIdentity identity;
        protected T component;

        [UnitySetUp]
        public IEnumerator SetupHost() => RunAsync(async () =>
        {
            networkManagerGo = new GameObject();
            networkManagerGo.AddComponent<MockTransport>();
            sceneManager = networkManagerGo.AddComponent<NetworkSceneManager>();
            manager = networkManagerGo.AddComponent<NetworkHost>();
            //manager.LocalClient = networkManagerGo.GetComponent<NetworkClient>();
            //manager = networkManagerGo.GetComponent<NetworkServer>();
            //server = manager.server;
            //client = manager.client;
            server.sceneManager = sceneManager;
            client.sceneManager = sceneManager;
            //manager.startOnHeadless = false;

            // wait for client and server to initialize themselves
            await Task.Delay(1);

            // now start the host
            await manager.StartHost();

            playerGO = new GameObject();
            identity = playerGO.AddComponent<NetworkIdentity>();
            component = playerGO.AddComponent<T>();

            server.AddPlayerForConnection(server.LocalConnection, playerGO);

            client.Update();
        });

        [TearDown]
        public void ShutdownHost()
        {
            Object.DestroyImmediate(playerGO);
            manager.StopHost();
            Object.DestroyImmediate(networkManagerGo);
        }

        #endregion
    }
}
