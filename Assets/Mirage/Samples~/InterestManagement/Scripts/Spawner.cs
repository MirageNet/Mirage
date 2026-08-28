using UnityEngine;

namespace Mirage.Examples.InterestManagement
{
    /// <summary>
    /// Spawns <see cref="count"/> copies of a prefab at random positions when the server starts
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        public int count = 50;
        public Bounds bounds;

        [RequiredField]
        public NetworkIdentity prefab;
        [RequiredField]
        public NetworkServer server;
        [RequiredField]
        public ServerObjectManager serverObjectManager;

        private void Awake()
        {
            server.Started.AddListener(Spawn);
        }

        private void Spawn()
        {
            for (var i = 0; i < count; i++)
                SpawnPrefab();
        }

        private void SpawnPrefab()
        {
            var position = new Vector3(
                (Random.value - 0.5f) * bounds.size.x + bounds.center.x,
                (Random.value - 0.5f) * bounds.size.y + bounds.center.y,
                (Random.value - 0.5f) * bounds.size.z + bounds.center.z
            );

            var newLoot = GameObject.Instantiate(prefab, position, Quaternion.identity, transform);

            serverObjectManager.Spawn(newLoot);
        }
    }
}