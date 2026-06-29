using UnityEngine;
using Mirage;

namespace Mirage.Snippets.SceneLoading
{
    public class CustomSceneLoader : MonoBehaviour
    {
        public NetworkServer Server;

        // CodeEmbed-Start: fixed-match-size
        // Modify this inside your duplicated class
        private void HandleSceneReadyMessage(INetworkPlayer player, SceneReadyMessage message)
        {
            player.SceneIsReady = true;

            // Check if everyone has finished loading
            bool allReady = true;
            foreach (var p in Server.AuthenticatedPlayers)
            {
                if (!p.SceneIsReady)
                {
                    allReady = false;
                    break;
                }
            }

            // Only spawn characters once everyone is fully loaded
            if (allReady)
                foreach (var p in Server.AuthenticatedPlayers)
                    SpawnCharacterForPlayer(p);
        }
        // CodeEmbed-End: fixed-match-size

        private void SpawnCharacterForPlayer(INetworkPlayer player)
        {
        }
    }
}
