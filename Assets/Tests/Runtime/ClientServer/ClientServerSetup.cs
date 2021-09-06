using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using UnityEngine;
using UnityEngine.TestTools;

using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ClientServerSetup<T> where T : NetworkBehaviour
    {

        protected GameObject serverGo;
        protected NetworkServer server;
        protected NetworkSceneManager serverSceneManager;
        protected ServerObjectManager serverObjectManager;
        protected GameObject serverPlayerGO;
        protected NetworkIdentity serverIdentity;
        protected T serverComponent;

        protected GameObject clientGo;
        protected NetworkClient client;
        protected NetworkSceneManager clientSceneManager;
        protected ClientObjectManager clientObjectManager;
        protected GameObject clientPlayerGO;
        protected NetworkIdentity clientIdentity;
        protected T clientComponent;

        protected GameObject playerPrefab;

        protected TestSocketFactory socketFactory;
        protected INetworkPlayer clientPlayer;
        protected INetworkPlayer serverPlayer;
        protected MessageHandler ClientMessageHandler => client.MessageHandler;
        protected MessageHandler ServerMessageHandler => server.MessageHandler;

        public virtual void ExtraSetup() { }

        protected virtual bool AutoConnectClient => true;
        protected virtual Config ServerConfig => null;
        protected virtual Config ClientConfig => null;

        [UnitySetUp]
        public IEnumerator Setup() => UniTask.ToCoroutine(async () =>
        {
            serverGo = new GameObject("server", typeof(NetworkSceneManager), typeof(ServerObjectManager), typeof(NetworkServer));
            clientGo = new GameObject("client", typeof(NetworkSceneManager), typeof(ClientObjectManager), typeof(NetworkClient));
            socketFactory = serverGo.AddComponent<TestSocketFactory>();

            await UniTask.Delay(1);

            server = serverGo.GetComponent<NetworkServer>();
            client = clientGo.GetComponent<NetworkClient>();

            if (ServerConfig != null) server.PeerConfig = ServerConfig;
            if (ClientConfig != null) client.PeerConfig = ClientConfig;

            server.SocketFactory = socketFactory;
            client.SocketFactory = socketFactory;

            serverSceneManager = serverGo.GetComponent<NetworkSceneManager>();
            clientSceneManager = clientGo.GetComponent<NetworkSceneManager>();
            serverSceneManager.Server = server;
            clientSceneManager.Client = client;
            serverSceneManager.Start();
            clientSceneManager.Start();

            serverObjectManager = serverGo.GetComponent<ServerObjectManager>();
            serverObjectManager.Server = server;
            serverObjectManager.NetworkSceneManager = serverSceneManager;
            serverObjectManager.Start();

            clientObjectManager = clientGo.GetComponent<ClientObjectManager>();
            clientObjectManager.Client = client;
            clientObjectManager.NetworkSceneManager = clientSceneManager;
            clientObjectManager.Start();

            ExtraSetup();

            // create and register a prefab
            playerPrefab = new GameObject("serverPlayer", typeof(NetworkIdentity), typeof(T));
            NetworkIdentity identity = playerPrefab.GetComponent<NetworkIdentity>();
            identity.AssetId = Guid.NewGuid();
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

                // get the connections so that we can spawn players
                serverPlayer = server.Players.First();
                clientPlayer = client.Player;

                // create a player object in the server
                serverPlayerGO = Object.Instantiate(playerPrefab);
                serverIdentity = serverPlayerGO.GetComponent<NetworkIdentity>();
                serverComponent = serverPlayerGO.GetComponent<T>();
                serverObjectManager.AddCharacter(serverPlayer, serverPlayerGO);

                // wait for client to spawn it
                await AsyncUtil.WaitUntilWithTimeout(() => clientPlayer.HasCharacter);

                clientIdentity = clientPlayer.Identity;
                clientPlayerGO = clientIdentity.gameObject;
                clientComponent = clientPlayerGO.GetComponent<T>();
            }
        });

        public virtual void ExtraTearDown() { }

        [UnityTearDown]
        public IEnumerator ShutdownHost() => UniTask.ToCoroutine(async () =>
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

            ExtraTearDown();
        });
    }
}
