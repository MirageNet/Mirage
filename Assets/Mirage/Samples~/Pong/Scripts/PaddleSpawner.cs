using UnityEngine;

namespace Mirage.Examples.Pong
{
    /// <summary>
    /// A lightweight character and ball spawner for Pong.
    /// Spawns rackets at the left/right spawn points and serves the ball once two players connect.
    /// </summary>
    public class PaddleSpawner : MonoBehaviour
    {
        [Header("References")]
        [RequiredField(autoFind: true)]
        public NetworkServer Server;
        [RequiredField(autoFind: true)]
        public ServerObjectManager ServerObjectManager;

        [Header("Character Spawning")]
        [RequiredField]
        public NetworkIdentity PlayerPrefab;
        [RequiredField]
        public Transform leftRacketSpawn;
        [RequiredField]
        public Transform rightRacketSpawn;
        [RequiredField]
        public GameObject ballPrefab;

        // track which player owns each side, so a new player fills the side that is free
        private INetworkPlayer _leftPlayer;
        private INetworkPlayer _rightPlayer;
        private GameObject _ball;

        private void Awake()
        {
            Server.Authenticated.AddListener(OnServerAuthenticated);
            Server.Disconnected.AddListener(OnServerDisconnect);
            Server.Stopped.AddListener(OnServerStopped);
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

            Transform start;
            if (_leftPlayer == null)
            {
                _leftPlayer = player;
                start = leftRacketSpawn;
            }
            else if (_rightPlayer == null)
            {
                _rightPlayer = player;
                start = rightRacketSpawn;
            }
            else
            {
                // both sides taken, extra players can watch but don't get a paddle.
                // AddCharacter normally sends existing objects, so without a character we need to send them ourselves
                ServerObjectManager.SpawnVisibleObjects(player, ignoreHasCharacter: true);
                return;
            }

            var character = Instantiate(PlayerPrefab, start.position, start.rotation);
            ServerObjectManager.AddCharacter(player, character.gameObject);

            // Serve the ball once both sides have a paddle
            if (_leftPlayer != null && _rightPlayer != null)
            {
                _ball = Instantiate(ballPrefab);
                ServerObjectManager.Spawn(_ball);
            }
        }

        private void OnServerDisconnect(INetworkPlayer player)
        {
            if (player == _leftPlayer)
                _leftPlayer = null;
            else if (player == _rightPlayer)
                _rightPlayer = null;
            else
                // spectator left, match can continue
                return;

            // Reset the match by destroying the ball when either paddle player leaves
            if (_ball != null)
                ServerObjectManager.Destroy(_ball);
        }

        private void OnServerStopped()
        {
            // all objects are destroyed when server stops, just clear the sides for the next session
            _leftPlayer = null;
            _rightPlayer = null;
        }
    }
}
