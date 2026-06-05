using Mirage;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Tests.Performance.Runtime.SpatialHashBenchmark
{
    [RequireComponent(typeof(NetworkClient))]
    public class ClientMonsterPool : MonoBehaviour
    {
        public Monster prefab;
        private NetworkClient client;
        private ClientObjectManager clientObjectManager;
        private Pool<Monster> pool;

        private void Awake()
        {
            client = GetComponent<NetworkClient>();
            clientObjectManager = GetComponent<ClientObjectManager>();

            client.Started.AddListener(ClientStarted);
        }

        private void ClientStarted()
        {
            var parent = new GameObject("Pool");
            parent.transform.parent = transform;
            pool = MonsterPool.CreatePool(prefab, parent.transform);
            clientObjectManager.RegisterSpawnHandler(prefab.Identity, SpawnMonster, UnspawnMonster);
        }

        private NetworkIdentity SpawnMonster(SpawnMessage msg)
        {
            var clone = pool.Take();
            clone.gameObject.SetActive(true);
            return clone.Identity;
        }

        private void UnspawnMonster(NetworkIdentity spawned)
        {
            var monster = spawned.GetComponent<Monster>();
            monster.gameObject.SetActive(false);
            pool.Put(monster);
        }
    }
}
