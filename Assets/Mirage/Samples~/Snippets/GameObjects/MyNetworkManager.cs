using UnityEngine;
using Mirage;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: spawning-without-network-manager-1
    public class MyNetworkManager : MonoBehaviour 
    {
        public GameObject treePrefab;
        public ClientObjectManager ClientObjectManager;
        public NetworkClient NetworkClient;
        public NetworkServer NetworkServer;
        public ServerObjectManager ServerObjectManager;

        void Start()
        {
            ClientObjectManager = FindObjectOfType<ClientObjectManager>();
            NetworkClient = FindObjectOfType<NetworkClient>();
            NetworkServer = FindObjectOfType<NetworkServer>();
            ServerObjectManager = FindObjectOfType<ServerObjectManager>();
        }

        // Register prefab and connect to the server  
        public void ClientConnect()
        {
            ClientObjectManager.spawnPrefabs.Add(treePrefab);
            NetworkClient.Connect("localhost");
            NetworkClient.MessageHandler.RegisterHandler<ConnectMessage>(OnClientConnect);
        }

        void OnClientConnect(NetworkConnection conn, ConnectMessage msg)
        {
            Debug.Log("Connected to server: " + conn);
        }
        // CodeEmbed-End: spawning-without-network-manager-1

        // CodeEmbed-Start: spawning-without-network-manager-2
        public void ServerListen()
        {
            // start listening, and allow up to 4 connections
            NetworkServer.StartServer();

            NetworkServer.MessageHandler.RegisterHandler<ConnectMessage>(OnServerConnect);
            NetworkServer.MessageHandler.RegisterHandler<ReadyMessage>(OnClientReady);
        }

        // When client is ready spawn a few trees  
        void OnClientReady(NetworkConnection conn, ReadyMessage msg)
        {
            Debug.Log("Client is ready to start: " + conn);
            SpawnTrees();
        }

        void SpawnTrees()
        {
            int x = 0;
            for (int i = 0; i < 5; ++i)
            {
                GameObject treeGo = Instantiate(treePrefab, new Vector3(x++, 0, 0), Quaternion.identity);
                ServerObjectManager.Spawn(treeGo);
            }
        }

        void OnServerConnect(NetworkConnection conn, ConnectMessage msg)
        {
            Debug.Log("New client connected: " + conn);
        }
        // CodeEmbed-End: spawning-without-network-manager-2
    }
}
