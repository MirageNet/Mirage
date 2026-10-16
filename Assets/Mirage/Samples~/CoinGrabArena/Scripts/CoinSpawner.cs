using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Examples.CoinGrabArena
{
    /// <summary>
    /// Server-only spawner that periodically places coins at random obstacle-free positions in the arena.
    /// </summary>
    public class CoinSpawner : MonoBehaviour
    {
        private const int MaxPlacementAttempts = 15;

        [Header("References")]
        [RequiredField(autoFind: true)]
        public NetworkServer Server;
        [RequiredField(autoFind: true)]
        public ServerObjectManager ServerObjectManager;
        [RequiredField]
        public CoinGrabGameManager GameManager;
        [RequiredField]
        public Coin CoinPrefab;

        [Header("Spawn Settings")]
        public int MaxCoins = 10;
        public float CoinSpawnInterval = 1.5f;
        public float ArenaHalfExtent = 12f;
        public float CoinHeight = 0.75f;
        public float ObstacleCheckRadius = 1.0f;
        public LayerMask ObstacleLayer;

        [Header("Coin Values")]
        [Range(0f, 1f)]
        public float GoldCoinChance = 0.1f;
        public int NormalCoinValue = 1;
        public int GoldCoinValue = 5;

        private readonly List<Coin> _activeCoins = new List<Coin>();
        private float _nextSpawnTime;

        private void Awake()
        {
            Server.Stopped.AddListener(OnServerStopped);
        }

        private void OnDestroy()
        {
            if (Server != null)
                Server.Stopped.RemoveListener(OnServerStopped);
        }

        private void Update()
        {
            if (!Server.Active || GameManager.State != RoundState.Playing)
                return;

            if (_activeCoins.Count >= MaxCoins)
                return;

            if (Time.time < _nextSpawnTime)
                return;

            _nextSpawnTime = Time.time + CoinSpawnInterval;
            TrySpawnCoin();
        }

        public void ResetSpawnTimer()
        {
            _nextSpawnTime = Time.time + CoinSpawnInterval;
        }

        public void OnCoinCollected(Coin coin)
        {
            // If we were at the cap, start a fresh interval from now before spawning the replacement coin
            if (_activeCoins.Count == MaxCoins)
                _nextSpawnTime = Time.time + CoinSpawnInterval;

            _activeCoins.Remove(coin);
        }

        public void ClearAllCoins()
        {
            for (var i = _activeCoins.Count - 1; i >= 0; i--)
            {
                var coin = _activeCoins[i];
                if (coin != null && coin.Identity.IsSpawned)
                    ServerObjectManager.Destroy(coin.gameObject);
            }

            _activeCoins.Clear();
        }

        private void OnServerStopped()
        {
            _activeCoins.Clear();
        }

        private void TrySpawnCoin()
        {
            if (!TryFindFreePosition(out var spawnPos))
                return;

            var isGold = Random.value < GoldCoinChance;
            var value = isGold ? GoldCoinValue : NormalCoinValue;

            var coin = Instantiate(CoinPrefab, spawnPos, Quaternion.identity);

            // Set SyncVars via InitializeServer before Spawn so clients receive them in the SpawnMessage
            coin.InitializeServer(this, GameManager, value, isGold);
            ServerObjectManager.Spawn(coin.gameObject);

            _activeCoins.Add(coin);
        }

        private bool TryFindFreePosition(out Vector3 position)
        {
            for (var i = 0; i < MaxPlacementAttempts; i++)
            {
                var x = Random.Range(-ArenaHalfExtent, ArenaHalfExtent);
                var z = Random.Range(-ArenaHalfExtent, ArenaHalfExtent);
                var candidate = new Vector3(x, CoinHeight, z);

                // Ignore triggers so existing coins do not count as solid obstacles, while still avoiding walls/pillars
                if (!Physics.CheckSphere(candidate, ObstacleCheckRadius, ObstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    position = candidate;
                    return true;
                }
            }

            position = default;
            return false;
        }
    }
}
