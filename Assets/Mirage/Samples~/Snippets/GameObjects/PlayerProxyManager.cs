using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    public class PlayerProxyManager : MonoBehaviour
    {
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;
        public NetworkIdentity PlayerProxyPrefab;
        public PlayerCharacter CharacterPrefab;
        public Transform spawnPoint;

        // CodeEmbed-Start: player-proxy-spawn-proxy
        private void OnServerAuthenticated(INetworkPlayer player)
        {
            var proxy = Instantiate(PlayerProxyPrefab);
            ServerObjectManager.AddCharacter(player, proxy.gameObject);
        }
        // CodeEmbed-End: player-proxy-spawn-proxy

        // CodeEmbed-Start: player-proxy-spawn-character
        private void SpawnGameplayCharacter(INetworkPlayer player)
        {
            var proxy = player.Identity.GetComponent<PlayerContext>();

            var character = Instantiate(CharacterPrefab, spawnPoint.position, spawnPoint.rotation);
            ServerObjectManager.Spawn(character.Identity, player);

            proxy.ActiveCharacter = character;
        }
        // CodeEmbed-End: player-proxy-spawn-character

        // CodeEmbed-Start: player-proxy-respawn
        private void RespawnCharacter(INetworkPlayer player)
        {
            var proxy = player.Identity.GetComponent<PlayerContext>();

            if (proxy.ActiveCharacter != null)
            {
                ServerObjectManager.Destroy(proxy.ActiveCharacter.gameObject);
                proxy.ActiveCharacter = null;
            }

            SpawnGameplayCharacter(player);
        }
        // CodeEmbed-End: player-proxy-respawn
    }
}
