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
        protected NetworkServer server;
        protected NetworkClient client;
        protected NetworkSceneManager sceneManager;

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
            networkManagerGo = new GameObject("Network", typeof(LoopbackTransport), typeof(NetworkClient), typeof(NetworkServer), typeof(NetworkSceneManager));

            sceneManager = networkManagerGo.GetComponent<NetworkSceneManager>();
            client = networkManagerGo.GetComponent<NetworkClient>();
            server = networkManagerGo.GetComponent<NetworkServer>();

            server.sceneManager = sceneManager;
            client.sceneManager = sceneManager;

            ExtraSetup();

            // create and register a prefab
            playerPrefab = new GameObject("serverPlayer", typeof(NetworkIdentity), typeof(T));
            playerPrefab.GetComponent<NetworkIdentity>().AssetId = Guid.NewGuid();
            client.RegisterPrefab(playerPrefab);

            // wait for client and server to initialize themselves
            await Task.Delay(1);

            // start the server
            await server.ListenAsync();

            // now start the client
            var builder = new UriBuilder
            {
                Host = "localhost",
                Scheme = client.Transport.Scheme,
            };

            await client.ConnectAsync(builder.Uri);

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
            client.Disconnect();
            server.Disconnect();

            yield return null;

            Object.DestroyImmediate(playerPrefab);
            Object.DestroyImmediate(networkManagerGo);
            Object.DestroyImmediate(serverPlayerGO);
            Object.DestroyImmediate(clientPlayerGO);
        }

        #endregion
    }
}
