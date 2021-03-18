using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Examples.InterestManagement
{
    public class Spawner : MonoBehaviour
    {
        public int count = 50;
        public Bounds bounds;

        public NetworkIdentity prefab;
        public ServerObjectManager serverObjectManager;

        public void Spawn()
        {
            for (int i = 0; i < count; i++)
                SpawnPrefab();
        }

        public void SpawnPrefab()
        {
            Vector3 position = new Vector3(
                (Random.value - 0.5f) * bounds.size.x + bounds.center.x,
                (Random.value - 0.5f) * bounds.size.y + bounds.center.y,
                (Random.value - 0.5f) * bounds.size.z + bounds.center.z
            );

            var newLoot = GameObject.Instantiate(prefab, position, Quaternion.identity, transform);

            serverObjectManager.Spawn(newLoot);
        }
    }
}