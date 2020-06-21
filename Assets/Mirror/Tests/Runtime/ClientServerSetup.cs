using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

using static Mirror.Tests.AsyncUtil;
using Object = UnityEngine.Object;

namespace Mirror.Tests
{
    // set's up a host
    public class ClientServerSetup<T> where T : NetworkBehaviour
    {
        #region Setup
        protected GameObject networkManagerGo;
        protected NetworkManager manager;
        protected NetworkServer server;
        protected NetworkClient client;

        protected GameObject serverPlayerGO;
        protected NetworkIdentity serverIdentity;
        protected T serverComponent;

        protected GameObject clientPlayerGO;
        protected NetworkIdentity clientIdentity;
        protected T clientComponent;

        private GameObject playerPrefab;
        protected INetworkConnection connectionToServer;
        protected INetworkConnection connectionToClient;

        public virtual void ExtraSetup()
        {

        }

        [UnitySetUp]
        public IEnumerator Setup() => RunAsync(async () =>
        {
            networkManagerGo = new GameObject();
            networkManagerGo.AddComponent<LoopbackTransport>();
            manager = networkManagerGo.AddComponent<NetworkManager>();
            manager.client = networkManagerGo.GetComponent<NetworkClient>();
            manager.server = networkManagerGo.GetComponent<NetworkServer>();

            server = manager.server;
            client = manager.client;
            manager.startOnHeadless = false;

            await Task.Delay(1);

            ExtraSetup();

            // start the server
            await manager.StartServer();

            // now start the client
            await manager.StartClient("localhost");

            // create and register a prefab
            playerPrefab = new GameObject();
            playerPrefab.AddComponent<NetworkIdentity>();
            playerPrefab.AddComponent<T>();

            await Task.Delay(1);

            playerPrefab.GetComponent<NetworkIdentity>().AssetId = Guid.NewGuid();
            client.RegisterPrefab(playerPrefab);

            // wait for client and server to initialize themselves
            await Task.Delay(1);

            // get the connections so that we can spawn players
            connectionToServer = client.Connection;
            connectionToClient = server.connections.First();

            // create a player object in the server
            serverPlayerGO = GameObject.Instantiate(playerPrefab);
            serverIdentity = serverPlayerGO.GetComponent<NetworkIdentity>();
            serverComponent = serverPlayerGO.GetComponent<T>();
            server.AddPlayerForConnection(connectionToClient, serverPlayerGO);

            // wait for client to spawn it
            await Task.Delay(1);

            clientPlayerGO = connectionToServer.Identity.gameObject;
            clientIdentity = clientPlayerGO.GetComponent<NetworkIdentity>();
            clientComponent = clientPlayerGO.GetComponent<T>();
        });

        [UnityTearDown]
        public IEnumerator ShutdownHost()
        {
            manager.StopClient();
            manager.StopServer();

            yield return null;

            networkManagerGo = null;
            serverPlayerGO = null;
            clientPlayerGO = null;
            playerPrefab = null;

            connectionToServer = null;
            connectionToClient = null;

            yield return null;

            Object.Destroy(playerPrefab);
            Object.Destroy(networkManagerGo);
            Object.Destroy(serverPlayerGO);
            Object.Destroy(clientPlayerGO);

            yield return null;
        }

        #endregion
    }
}
