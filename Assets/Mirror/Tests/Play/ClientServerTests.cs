using UnityEngine;

namespace Mirror.Tests
{
    public class ClientServerTests
    {
        #region Setup
        protected GameObject networkManagerGo;
        protected NetworkManager manager;
        protected NetworkServer server;
        protected NetworkClient client;

        protected GameObject clientNetworkManagerGo;
        protected NetworkManager clientManager;
        protected NetworkServer server2;
        protected NetworkClient client2;

        public void SetupServer()
        {
            networkManagerGo = new GameObject();
            manager = networkManagerGo.AddComponent<NetworkManager>();
            manager.Client = networkManagerGo.GetComponent<NetworkClient>();
            manager.Server = networkManagerGo.GetComponent<NetworkServer>();
            server = manager.Server;
            client = manager.Client;

            manager.AutoCreatePlayer = false;
            
            manager.StartServer();
        }

        public void SetupClient(string hostname = "localhost")
        {
            clientNetworkManagerGo = new GameObject();
            clientManager = clientNetworkManagerGo.AddComponent<NetworkManager>();
            clientManager.Client = clientNetworkManagerGo.GetComponent<NetworkClient>();
            clientManager.Server = clientNetworkManagerGo.GetComponent<NetworkServer>();
            server2 = clientManager.Server;
            client2 = clientManager.Client;

            clientManager.StartClient(hostname);
        }

        public void SetupClient(System.Uri uri)
        {
            clientNetworkManagerGo = new GameObject();
            clientManager = clientNetworkManagerGo.AddComponent<NetworkManager>();
            clientManager.Client = clientNetworkManagerGo.GetComponent<NetworkClient>();
            clientManager.Server = clientNetworkManagerGo.GetComponent<NetworkServer>();
            server2 = clientManager.Server;
            client2 = clientManager.Client;

            clientManager.StartClient(uri);
        }

        public void ShutdownServer()
        {
            manager.StopServer();
            GameObject.DestroyImmediate(networkManagerGo);
        }

        public void ShutdownClient()
        {
            clientManager.StopClient();
            GameObject.DestroyImmediate(clientNetworkManagerGo);
        }

        #endregion
    }
}
