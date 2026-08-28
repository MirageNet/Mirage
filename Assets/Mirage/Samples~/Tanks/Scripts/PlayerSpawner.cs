using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Examples.Tanks
{
    /// <summary>
    /// Spawns a character for each player at the next spawn point.
    /// <para>
    /// Based on <see cref="Mirage.Components.NetworkSceneLoader"/>, with scene loading removed because this example only uses 1 scene.
    /// Copy and modify it if you need custom spawn logic.
    /// </para>
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("References")]
        [RequiredField(autoFind: true)]
        public NetworkServer Server;
        [RequiredField(autoFind: true)]
        public ServerObjectManager ServerObjectManager;

        [Header("Character Spawning")]
        [RequiredField]
        public NetworkIdentity PlayerPrefab;
        [Tooltip("Players are spawned at each point in turn. If empty, players are spawned at the prefab's position")]
        public List<Transform> SpawnPoints = new List<Transform>();

        private int _nextSpawnIndex;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Authenticated.AddListener(OnServerAuthenticated);
        }

        private void OnServerStarted()
        {
            // start from the first spawn point each session
            _nextSpawnIndex = 0;
        }

        private void OnServerAuthenticated(INetworkPlayer player)
        {
            // client is already in the only scene, so it is ready as soon as it is authenticated
            player.SceneIsReady = true;

            SpawnCharacterForPlayer(player);
        }

        private void SpawnCharacterForPlayer(INetworkPlayer player)
        {
            Debug.Assert(player.Identity == null, "Player already has a character spawned.");

            NetworkIdentity character;
            if (SpawnPoints.Count > 0)
            {
                var spawnPoint = SpawnPoints[_nextSpawnIndex];
                _nextSpawnIndex = (_nextSpawnIndex + 1) % SpawnPoints.Count;
                character = Instantiate(PlayerPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                character = Instantiate(PlayerPrefab);
            }

            // AddCharacter spawns the object on clients and assigns authority to the player
            ServerObjectManager.AddCharacter(player, character.gameObject);
        }
    }
}
