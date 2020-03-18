using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{

    // set's up a host

    public class HostTests<T> where T:NetworkBehaviour
    {

        #region Setup
        protected GameObject networkManagerGo;
        protected NetworkManager manager;
        protected NetworkServer server;
        protected NetworkClient client;

        protected GameObject playerGO;
        protected NetworkIdentity identity;
        protected T component;

        [SetUp]
        public void SetupHost()
        {
            networkManagerGo = new GameObject();
            manager = networkManagerGo.AddComponent<NetworkManager>();
            manager.client = networkManagerGo.GetComponent<NetworkClient>();
            manager.server = networkManagerGo.GetComponent<NetworkServer>();
            server = manager.server;
            client = manager.client;
            server.Transport2 = networkManagerGo.GetComponent<Transport2>();
            client.Transport = networkManagerGo.GetComponent<Transport2>();

            manager.autoCreatePlayer = false;

            manager.StartHost();

            playerGO = new GameObject();
            identity = playerGO.AddComponent<NetworkIdentity>();
            component = playerGO.AddComponent<T>();

            server.AddPlayerForConnection(server.localConnection, playerGO);

            client.Update();
        }

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
