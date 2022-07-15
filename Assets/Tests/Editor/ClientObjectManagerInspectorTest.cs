using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests
{
    [TestFixture(Category = "ClientObjectManager")]
    public class ClientObjectManagerInspectorTest
    {

        [Test]
        public void RegisterPrefabs()
        {
            var gameObject = new GameObject("NetworkObjectManager", typeof(ClientObjectManager));

            var client = gameObject.GetComponent<ClientObjectManager>();

            var inspector = ScriptableObject.CreateInstance<ClientObjectManagerInspector>();
            inspector.RegisterPrefabs(client);

            Assert.That(client.spawnPrefabs, Has.Count.GreaterThan(2));
            GameObject.DestroyImmediate(gameObject);
            GameObject.DestroyImmediate(inspector);
        }

        [Test]
        public void PreserveExisting()
        {
            var preexisting = new GameObject("object", typeof(NetworkIdentity));

            var gameObject = new GameObject("NetworkObjectManager", typeof(ClientObjectManager));
            var client = gameObject.GetComponent<ClientObjectManager>();
            client.spawnPrefabs.Add(preexisting.GetComponent<NetworkIdentity>());

            var inspector = ScriptableObject.CreateInstance<ClientObjectManagerInspector>();

            inspector.RegisterPrefabs(client);

            Assert.That(client.spawnPrefabs, Contains.Item(preexisting.GetComponent<NetworkIdentity>()));

            GameObject.DestroyImmediate(gameObject);
            GameObject.DestroyImmediate(preexisting);
        }
    }
}
