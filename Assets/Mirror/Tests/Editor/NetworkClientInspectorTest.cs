using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{
    [TestFixture(Category = "NetworkClient")]
    public class NetworkClientInspectorTest
    {
        [Test]
        public void RegisterPrefabs()
        {
            var gameObject = new GameObject("NetworkClient", typeof(ClientObjectManager));

            ClientObjectManager clientObjMgr = gameObject.GetComponent<ClientObjectManager>();

            NetworkClientInspector inspector = ScriptableObject.CreateInstance<NetworkClientInspector>();
            inspector.RegisterPrefabs(clientObjMgr);

            Assert.That(clientObjMgr.spawnPrefabs, Has.Count.GreaterThan(13));

            foreach (var prefab in clientObjMgr.spawnPrefabs)
            {
                Assert.That(prefab.GetComponent<NetworkIdentity>(), Is.Not.Null);
            }
            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void PreserveExisting()
        {
            var preexisting = new GameObject("object", typeof(NetworkIdentity));

            var gameObject = new GameObject("NetworkClient", typeof(ClientObjectManager));
            ClientObjectManager clientObjMgr = gameObject.GetComponent<ClientObjectManager>();
            clientObjMgr.spawnPrefabs.Add(preexisting);

            NetworkClientInspector inspector = ScriptableObject.CreateInstance<NetworkClientInspector>();

            inspector.RegisterPrefabs(clientObjMgr);

            Assert.That(clientObjMgr.spawnPrefabs, Contains.Item(preexisting));

            GameObject.DestroyImmediate(gameObject);
            GameObject.DestroyImmediate(preexisting);
        }
    }
}
