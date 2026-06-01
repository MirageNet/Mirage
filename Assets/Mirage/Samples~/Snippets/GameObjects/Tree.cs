using UnityEngine;
using Mirage;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: tree-syncvar-example
    public class Tree : NetworkBehaviour
    {
        [SyncVar]
        public int numLeaves { get; set; }

        void Start()
        {
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        public void OnStartClient()
        {
            Debug.Log("Tree spawned with leaf count " + numLeaves);
        }
    }
    // CodeEmbed-End: tree-syncvar-example

    public class TreeAuthorityExample : NetworkBehaviour
    {
        public GameObject treePrefab;
        public ClientObjectManager ClientObjectManager;
        public NetworkClient NetworkClient;
        public int numLeaves;

        private void OnClientConnect(NetworkConnection conn, ConnectMessage msg) {}

        // CodeEmbed-Start: tree-client-authority
        public void ClientConnect()
        {
            ClientObjectManager.spawnPrefabs.Add(treePrefab);
            NetworkClient.Connect("localhost");
            NetworkClient.MessageHandler.RegisterHandler<ConnectMessage>(OnClientConnect);

            NetworkClient.Player.Identity.OnAuthorityChanged.AddListener(OnStartAuthority);
        }

        public void OnStartAuthority(bool changed)
        {
            CmdMessageFromTree("Tree with " + numLeaves + " reporting in");
        }

        [ServerRpc]
        void CmdMessageFromTree(string msg)
        {
            Debug.Log("Client sent a tree message: " + msg);
        }
        // CodeEmbed-End: tree-client-authority
    }
}
