using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: spawning-without-network-manager-1
    public class MyNetworkManager : MonoBehaviour
    {
        // Assign values in inspector
        public NetworkIdentity treePrefab;
        public ClientObjectManager ClientObjectManager;
        public NetworkClient Client;
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;

        private void Awake()
        {
            // it is best to add events once in awake
            // this avoids the need to remove or clean them up,
            // which is ok for Manager classes that live as long as the NetworkServer/Client themselves

            Server.Started.AddListener(OnServerStarted);
            // use Authenticated instead of Connected to ensure that player is fully setup
            Server.Authenticated.AddListener(OnServerConnect);
            Client.Authenticated.AddListener(OnClientConnect);
        }

        // Register prefab and connect to the server
        // Call this from your UI or other code
        public void StartClient(string address)
        {
            ClientObjectManager.spawnPrefabs.Add(treePrefab);

            Client.Connect(address);
        }

        private void OnClientConnect(INetworkPlayer player)
        {
            Debug.Log("Connected to server: " + player);
        }
        // CodeEmbed-End: spawning-without-network-manager-1

        // CodeEmbed-Start: spawning-without-network-manager-2
        public void StartServer()
        {
            // start listening
            Server.StartServer();
        }

        private void OnServerStarted()
        {
            // it is best to register message from .Started event
            // this means they will be added early enough for host player to use them
            Server.MessageHandler.RegisterHandler<SceneReadyMessage>(HandleSceneReadyMessage);
        }

        // When client is ready spawn a few trees  
        private void HandleSceneReadyMessage(INetworkPlayer player, SceneReadyMessage msg)
        {
            Debug.Log("Client is ready to start: " + player);
            SpawnTrees();
        }

        private void SpawnTrees()
        {
            var x = 0;
            for (var i = 0; i < 5; ++i)
            {
                var treeGo = Instantiate(treePrefab, new Vector3(x++, 0, 0), Quaternion.identity);
                ServerObjectManager.Spawn(treeGo);
            }
        }

        private void OnServerConnect(INetworkPlayer player)
        {
            Debug.Log("New client connected: " + player);
        }
        // CodeEmbed-End: spawning-without-network-manager-2
    }
}
