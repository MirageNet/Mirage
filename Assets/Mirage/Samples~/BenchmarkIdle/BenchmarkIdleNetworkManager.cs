using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JamesFrowen.Benchmarker;
using JamesFrowen.Benchmarker.Weaver;
using Mirage;
using UnityEngine;

namespace Mirror.Examples.BenchmarkIdle
{
    [AddComponentMenu("")]
    public class BenchmarkIdleNetworkManager : NetworkManager
    {
        [Header("Spawns")]

        public int PlayerCount;
        public int spawnAmount = 10_000;
        public float interleave = 1;
        public GameObject spawnPrefab;

        public GameObject PlayerPrefab;


        // player spawn positions should be spread across the world.
        // not all at one place.
        // but _some_ at the same place.
        // => deterministic random is ideal
        [Range(0, 1)] public float spawnPositionRatio = 0.01f;
        private System.Random random = new System.Random(42);
        private List<Npc> npcs = new List<Npc>();
        private readonly List<GameObject> _startPosition = new List<GameObject>();
        private bool _benchmarkStarted;
        private void Awake()
        {
            Server.Started.AddListener(OnStartServer);
            Server.Authenticated.AddListener(SpawnPlayer);
            Server.Authenticated.AddListener(OnServerConnected);

            Client.Disconnected.AddListener(_ => Quit());
            Camera.main.enabled = false;
        }

        private void OnStartServer()
        {
            _startPosition.Clear();
            SpawnAll();
        }

        private void OnServerConnected(INetworkPlayer _)
        {
            if (Server.Players.Count == PlayerCount)
            {
                UniTask.Void(async () =>
                {
                    await UniTask.Delay(1000);

                    Server.ManualUpdate = true;
                    BenchmarkRunner.StartRecording(3000, true, true);
                    BenchmarkRunner.MetaData = new List<string>()
                    {
                        $"PlayerCount:{PlayerCount}",
                        $"spawnAmount:{spawnAmount}",
                        $"Idle %:{npcs[0].sleepingProbability}",
#if GET_ID_CACHE
                        $"GET_ID_CACHE",
#endif
#if SET_DIRTY_NO_LOG
                        $"SET_DIRTY_NO_LOG",
#endif
#if WAS_ZERO
                        $"WAS_ZERO",
#endif
#if SHOULD_SYNC_CACHE
                        $"SHOULD_SYNC_CACHE",
#endif
#if DIRTY_SET_CACHE
                        $"DIRTY_SET_CACHE",
#elif DIRTY_LIST_CACHE
                        $"DIRTY_LIST_CACHE",
#endif
#if DIRTY_LIST
                        $"DIRTY_LIST",
#endif
#if NO_DIRTY_LIST
                        $"NO_DIRTY_LIST",
#endif
                        "SyncVarSender.Update Self Only",
                        "No_interval"
                    };


                    _benchmarkStarted = true;
                });
            }
        }

        private void SpawnAll()
        {
            // calculate sqrt so we can spawn N * N = Amount
            var sqrt = Mathf.Sqrt(spawnAmount);

            // calculate spawn xz start positions
            // based on spawnAmount * distance
            var offset = -sqrt / 2 * interleave;

            // spawn exactly the amount, not one more.
            var spawned = 0;
            for (var spawnX = 0; spawnX < sqrt; ++spawnX)
            {
                for (var spawnZ = 0; spawnZ < sqrt; ++spawnZ)
                {
                    // spawn exactly the amount, not any more
                    // (our sqrt method isn't 100% precise)
                    if (spawned < spawnAmount)
                    {
                        // spawn & position
                        var go = Instantiate(spawnPrefab);
                        var x = offset + (spawnX * interleave);
                        var z = offset + (spawnZ * interleave);
                        var position = new Vector3(x, 0, z);
                        go.transform.position = position;

                        // spawn
                        ServerObjectManager.Spawn(go);
                        ++spawned;

                        npcs.Add(go.GetComponent<Npc>());

                        // add random spawn position for players.
                        // don't have them all in the same place.
                        if (random.NextDouble() <= spawnPositionRatio)
                        {
                            var spawnGO = new GameObject("Spawn");
                            spawnGO.transform.position = position;
                            _startPosition.Add(spawnGO);
                        }
                    }
                }
            }
        }

        private void SpawnPlayer(INetworkPlayer player)
        {
            var clone = Instantiate(PlayerPrefab);
            var index = random.Next(0, _startPosition.Count);
            clone.transform.position = _startPosition[index].transform.position;
            _startPosition.RemoveAt(index);
            ServerObjectManager.AddCharacter(player, clone);
        }


        private void Update()
        {
            if (_benchmarkStarted && !BenchmarkHelper.IsRunning)
            {
                // finished
                Quit();
                return;
            }


            if (_benchmarkStarted && Server.Active)
                ServerUpdate();
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        [BenchmarkMethod()]
        private void ServerUpdate()
        {
            UpdateReceived();
            UpdateSyncVar_First();
            UpdateSyncVar_Second();
            UpdateSyncVar_Third();
            UpdateSent();
        }

        [BenchmarkMethod()]
        private void UpdateSyncVar_First()
        {
            foreach (var npc in npcs)
            {
                npc.Update_SetSyncVar();
            }
        }
        [BenchmarkMethod()]
        private void UpdateSyncVar_Second()
        {
            foreach (var npc in npcs)
            {
                npc.Update_SetSyncVar();
            }
        }
        [BenchmarkMethod()]
        private void UpdateSyncVar_Third()
        {
            foreach (var npc in npcs)
            {
                npc.Update_SetSyncVar();
            }
        }

        [BenchmarkMethod()]
        private void UpdateSent()
        {
            Server.UpdateSent();
        }

        [BenchmarkMethod()]
        private void UpdateReceived()
        {
            Server.UpdateReceive();
        }
    }
}
