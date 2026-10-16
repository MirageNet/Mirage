using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Examples.CoinGrabArena
{
    /// <summary>
    /// Spawns a <see cref="CoinGrabPlayer"/> for each authenticated player at the next spawn point.
    /// <para>
    /// Based on <see cref="Mirage.Components.NetworkSceneLoader"/> with scene loading removed because
    /// the arena runs in a single scene.
    /// </para>
    /// </summary>
    public class CoinGrabSpawner : MonoBehaviour
    {
        private static readonly Color32[] PlayerColors =
        {
            new Color32(59, 130, 246, 255),  // Blue
            new Color32(239, 68, 68, 255),   // Red
            new Color32(16, 185, 129, 255),  // Green
            new Color32(245, 158, 11, 255),  // Amber
            new Color32(168, 85, 247, 255),  // Purple
            new Color32(236, 72, 153, 255),  // Pink
            new Color32(6, 182, 212, 255),   // Cyan
            new Color32(249, 115, 22, 255),  // Orange
        };

        [Header("References")]
        [RequiredField(autoFind: true)]
        public NetworkServer Server;
        [RequiredField(autoFind: true)]
        public ServerObjectManager ServerObjectManager;

        [Header("Character Spawning")]
        [RequiredField]
        public CoinGrabPlayer PlayerPrefab;
        [Tooltip("Players are spawned at each point in turn. If empty, players spawn at the prefab position.")]
        public List<Transform> SpawnPoints = new List<Transform>();

        private int _nextSpawnIndex;
        private int _nextPlayerNumber;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Authenticated.AddListener(OnServerAuthenticated);
        }

        private void OnDestroy()
        {
            if (Server != null)
            {
                Server.Started.RemoveListener(OnServerStarted);
                Server.Authenticated.RemoveListener(OnServerAuthenticated);
            }
        }

        private void OnServerStarted()
        {
            // Reset indices each session so the first player starts at SpawnPoints[0] as Player 1
            _nextSpawnIndex = 0;
            _nextPlayerNumber = 1;
        }

        private void OnServerAuthenticated(INetworkPlayer player)
        {
            // Single-scene game: the client already has the arena loaded, so mark it ready immediately
            player.SceneIsReady = true;

            SpawnCharacterForPlayer(player);
        }

        private void SpawnCharacterForPlayer(INetworkPlayer player)
        {
            Debug.Assert(player.Identity == null, "Player already has a character spawned.");

            GetNextSpawnPoint(out var position, out var rotation);
            var character = Instantiate(PlayerPrefab, position, rotation);

            // Set initial SyncVars before AddCharacter so they are packed into the initial SpawnMessage
            // instead of sending a separate update packet on the next frame
            var playerNumber = _nextPlayerNumber;
            _nextPlayerNumber++;

            character.PlayerName = $"Player {playerNumber}";
            character.Color = PlayerColors[(playerNumber - 1) % PlayerColors.Length];
            character.Score = 0;

            ServerObjectManager.AddCharacter(player, character.gameObject);
        }

        public void GetNextSpawnPoint(out Vector3 position, out Quaternion rotation)
        {
            if (SpawnPoints.Count > 0)
            {
                var point = SpawnPoints[_nextSpawnIndex % SpawnPoints.Count];
                _nextSpawnIndex = (_nextSpawnIndex + 1) % SpawnPoints.Count;
                position = point.position;
                rotation = point.rotation;
            }
            else
            {
                position = PlayerPrefab.transform.position;
                rotation = PlayerPrefab.transform.rotation;
            }
        }
    }
}
