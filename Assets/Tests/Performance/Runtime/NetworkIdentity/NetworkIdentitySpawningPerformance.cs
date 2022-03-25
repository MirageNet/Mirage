using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Mirage.Tests.Performance
{
    [Category("Performance")]
    [Category("Benchmark")]
    public class NetworkIdentitySpawningPerformance
    {
        List<GameObject> spawned = new List<GameObject>();
        GameObject prefab;

        GameObject Spawn()
        {
            var clone = GameObject.Instantiate(prefab);
            spawned.Add(clone);
            clone.SetActive(true);
            return clone;
        }

        [SetUp]
        public void SetUp()
        {
            prefab = new GameObject("NetworkPrefab");
            prefab.SetActive(false); // disable so that NetworkIdentity.Awake is not called
            prefab.AddComponent<NetworkIdentity>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(prefab);
            foreach (GameObject item in spawned)
                UnityEngine.Object.DestroyImmediate(item);

            spawned.Clear();
        }

        [Test]
        [Performance]
        public void SpawnManyObjects()
        {
            Debug.Log($"Debug build:{Debug.isDebugBuild}");
            Measure.Method(() =>
            {
                // spawn 100 objects
                for (int i = 0; i < 100; i++)
                {
                    _ = Spawn();
                }
            })
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
        }
    }
}

