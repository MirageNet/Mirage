using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{
    [TestFixture(Category = "NetworkObjectManager")]
    public class NetworkObjectManagerInspectorTest
    {

        [Test]
        public void RegisterPrefabs()
        {
            var gameObject = new GameObject("NetworkObjectManager", typeof(NetworkObjectManager));

            NetworkObjectManager client = gameObject.GetComponent<NetworkObjectManager>();

            NetworkObjectManagerInspector inspector = ScriptableObject.CreateInstance<NetworkObjectManagerInspector>();
            inspector.RegisterPrefabs(client);

            Assert.That(client.spawnPrefabs, Has.Count.GreaterThan(2));

            foreach (var prefab in client.spawnPrefabs)
            {
                Assert.That(prefab.GetComponent<NetworkIdentity>(), Is.Not.Null);
            }
            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void PreserveExisting()
        {
            var preexisting = new GameObject("object", typeof(NetworkIdentity));

            var gameObject = new GameObject("NetworkObjectManager", typeof(NetworkObjectManager));
            NetworkObjectManager client = gameObject.GetComponent<NetworkObjectManager>();
            client.spawnPrefabs.Add(preexisting);

            NetworkObjectManagerInspector inspector = ScriptableObject.CreateInstance<NetworkObjectManagerInspector>();

            inspector.RegisterPrefabs(client);

            Assert.That(client.spawnPrefabs, Contains.Item(preexisting));

            GameObject.DestroyImmediate(gameObject);
            GameObject.DestroyImmediate(preexisting);
        }
    }
}
