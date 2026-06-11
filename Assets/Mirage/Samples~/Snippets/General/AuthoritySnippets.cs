using UnityEngine;

namespace Mirage.Snippets.General
{
    public class AuthoritySnippets : NetworkBehaviour
    {
        public GameObject prefab;
        public ServerObjectManager ServerObjectManager;

        public void SpawnWithAuthority(INetworkPlayer owner)
        {
            // CodeEmbed-Start: authority-spawn-with-owner
            GameObject go = Instantiate(prefab);
            ServerObjectManager.Spawn(go, owner);
            // CodeEmbed-End: authority-spawn-with-owner
        }

        public void AssignAuthorityExample(INetworkPlayer conn)
        {
            // CodeEmbed-Start: authority-assign-client-authority
            Identity.AssignClientAuthority(conn);
            // CodeEmbed-End: authority-assign-client-authority
        }

        public INetworkPlayer connectionToClient => Owner;

        // CodeEmbed-Start: authority-pickup-item
        // Command on character object
        [ServerRpc]
        private void PickupItem(NetworkIdentity item)
        {
            item.AssignClientAuthority(connectionToClient); 
        }
        // CodeEmbed-End: authority-pickup-item

        public void RemoveAuthorityExample()
        {
            // CodeEmbed-Start: authority-remove-client-authority
            Identity.RemoveClientAuthority();
            // CodeEmbed-End: authority-remove-client-authority
        }
    }
}
