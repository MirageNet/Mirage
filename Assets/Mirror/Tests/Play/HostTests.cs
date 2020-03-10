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
            manager.Client = networkManagerGo.GetComponent<NetworkClient>();
            manager.Server = networkManagerGo.GetComponent<NetworkServer>();
            server = manager.Server;
            client = manager.Client;

            manager.AutoCreatePlayer = false;

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
            GameObject.DestroyImmediate(playerGO);
            manager.StopHost();
            GameObject.DestroyImmediate(networkManagerGo);
        }

        #endregion
    }
}
