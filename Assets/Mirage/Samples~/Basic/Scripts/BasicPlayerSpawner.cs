using UnityEngine;

namespace Mirage.Examples.Basic
{
    /// <summary>
    /// A lightweight character spawner for single-scene setups.
    /// Spawns and configures a player UI object as soon as the client authenticates with the server.
    /// </summary>
    public class BasicPlayerSpawner : MonoBehaviour
    {
        [Header("References")]
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;

        [Header("Character Spawning")]
        public NetworkIdentity PlayerPrefab;
        public Transform Parent;

        // counter to give each player their own id
        private int _playerCounter;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Authenticated.AddListener(OnServerAuthenticated);
        }

        private void OnServerStarted()
        {
            // Reset player numbering when the server starts so player IDs begin at 1 for each session
            _playerCounter = 0;
        }

        private void OnServerAuthenticated(INetworkPlayer player)
        {
            // In a single-scene example without scene transitions, the client is immediately ready on authentication
            player.SceneIsReady = true;

            SpawnCharacterForPlayer(player);
        }

        private void SpawnCharacterForPlayer(INetworkPlayer player)
        {
            Debug.Assert(player.Identity == null, "Player already has a character spawned.");

            // Instantiate under the UI parent canvas so the element appears in the layout hierarchy
            var character = Instantiate(PlayerPrefab, Parent);

            // Set initial SyncVars prior to AddCharacter so they are packed into the initial SpawnMessage
            var basicPlayer = character.GetComponent<BasicPlayer>();
            // increment first, so first player has id=1
            _playerCounter++;
            basicPlayer.playerNo = _playerCounter;

            // AddCharacter spawns the object on clients and assigns authority to the connection
            ServerObjectManager.AddCharacter(player, character.gameObject);
        }
    }
}
