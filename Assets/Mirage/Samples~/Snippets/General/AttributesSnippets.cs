using UnityEngine;
using Mirage;

namespace Mirage.Snippets.General
{
    public class AttributesSnippets : NetworkBehaviour
    {
        // CodeEmbed-Start: attributes-server
        [Server]
        private void SpawnCoin() 
        {
            // This method is only allowed to be invoked on the server.
        }
        // CodeEmbed-End: attributes-server

        // CodeEmbed-Start: attributes-network-method
        [NetworkMethod(NetworkFlags.Server | NetworkFlags.NotActive)]
        public void StartGame()
        {
            // This method will run on the server or in single-player mode.
            // It will only be blocked if the client is active.
        }
        // CodeEmbed-End: attributes-network-method
    }
}
