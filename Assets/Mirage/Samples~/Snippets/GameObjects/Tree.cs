using UnityEngine;
using Mirage;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: tree-syncvar-example
    public class Tree : NetworkBehaviour
    {
        [SyncVar]
        public int numLeaves;

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

}

namespace Mirage.Snippets.GameObjects.ClientAuthority
{
    // CodeEmbed-Start: tree-client-authority
    public class Tree : NetworkBehaviour
    {
        public int numLeaves;

        private void Awake()
        {
            // Register listener in Awake to catch any early authority changes.
            Identity.OnAuthorityChanged.AddListener(OnStartAuthority);
        }

        public void OnStartAuthority(bool hasAuthority)
        {
            // Only execute server command when we are given authority over the tree.
            if (hasAuthority)
                CmdMessageFromTree("Tree with " + numLeaves + " reporting in");
        }

        [ServerRpc]
        private void CmdMessageFromTree(string msg)
        {
            Debug.Log("Client sent a tree message: " + msg);
        }
    }
    // CodeEmbed-End: tree-client-authority
}
