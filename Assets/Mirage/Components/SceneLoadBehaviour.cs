using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Components
{
    public class SceneLoadBehaviour : NetworkBehaviour
    {
        [SerializeField] private float _clientTimeout = 60;
        [NetworkedPrefab] public GameObject Character;

        private List<INetworkPlayer> _players;
        public UniTask<List<INetworkPlayer>> Load(string scene, bool addCharacter)
        {
            return Load(scene, addCharacter, _clientTimeout);
        }
        public async UniTask<List<INetworkPlayer>> Load(string scene, bool addCharacter, float timeout)
        {
            if (!IsServer)
                throw new InvalidOperationException("Must be server to looad scene");

            _players = Server.Players.ToList();


            await LoadSceneWithTimeout(scene, timeout);

            // spawn new scene objects
            ServerObjectManager.SpawnSceneObjects();

            if (addCharacter)
            {
                foreach (var player in _players)
                {
                    var clone = Instantiate(Character);
                    ServerObjectManager.AddCharacter(player, clone);
                }
            }

            return _players;
        }

        private void _playerDisconnected(INetworkPlayer player)
        {
            _players.Remove(player);
            Debug.Log("Player disconnected while loading");
        }

        private async Task LoadSceneWithTimeout(string scene, float timeout)
        {
            var timeoutToken = new CancellationTokenSource();
            timeoutToken.CancelAfterSlim(TimeSpan.FromSeconds(timeout));
            try
            {
                try
                {
                    // add even to remove players from list if they get disconnected
                    Server.Disconnected.AddListener(_playerDisconnected);
                    await LoadScene(scene);
                }
                finally
                {
                    // remove event before we get into the catch block, since we will remove connections in there if they are still loading
                    Server.Disconnected.RemoveListener(_playerDisconnected);
                }
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == timeoutToken.Token)
                {
                    Debug.LogError("Client load timeout ");
                    RemoveLoadingPlayers();
                }
            }
        }

        private void RemoveLoadingPlayers()
        {
            for (var i = _players.Count - 1; i >= 0; i--)
            {
                if (!_players[i].SceneIsReady)
                {
                    _players.RemoveAt(i);
                    _players[i].Disconnect();
                }
            }
        }

        private async UniTask LoadScene(string scene)
        {
            foreach (var player in _players)
            {
                player.SceneIsReady = false;
            }

            RpcLoad(scene);
            Debug.Log($"LoadSceneAsync {scene}");
            var op = SceneManager.LoadSceneAsync(scene);
            await op.ToUniTask();

            // wait for all players to load
            while (true)
            {
                var allLoaded = true;
                foreach (var player in _players)
                {
                    if (!player.SceneIsReady)
                        allLoaded = false;
                }

                if (allLoaded)
                {
                    Debug.Log($"All players ready");
                    break;
                }

                await UniTask.Yield();
            }
        }

        [ClientRpc]
        private void RpcLoad(string scene)
        {
            Debug.Log($"RpcLoad {scene}");
            UniTask.Void(async () =>
            {
                Client.Player.SceneIsReady = false;

                Debug.Log($"LoadSceneAsync {scene}");
                var op = SceneManager.LoadSceneAsync(scene);
                await op.ToUniTask();

                Client.Player.SceneIsReady = true;
                ClientObjectManager.PrepareToSpawnSceneObjects();
                RpcSceneLoaded();
            });
        }

        [ServerRpc(requireAuthority = false)]
        private void RpcSceneLoaded(INetworkPlayer sender = null)
        {
            Debug.Log($"Player ready: {sender}");
            sender.SceneIsReady = true;
        }

        public void AddCharacters(List<INetworkPlayer> players, IEnumerable<Vector3> allSpawnPoints)
        {
            var spawnPoints = allSpawnPoints.ToList();
            if (players.Count > spawnPoints.Count)
            {
                throw new InvalidOperationException($"Not enough spawn points. players:{players.Count} spawnPoints:{spawnPoints.Count}");
            }

            foreach (var player in players)
            {
                // get random index from list, 
                // and then remove it from list after so we dont use it again
                var i = UnityEngine.Random.Range(0, spawnPoints.Count);
                var pos = spawnPoints[i];
                spawnPoints.RemoveAt(i);

                var clone = Instantiate(Character, pos, Quaternion.identity);
                ServerObjectManager.AddCharacter(player, clone);
            }
        }
    }
}
