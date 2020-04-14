using System.Collections;
using System.Collections.Generic;
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
            var gameObject = new GameObject("NetworkClient", typeof(NetworkClient));

            NetworkClient client = gameObject.GetComponent<NetworkClient>();

            client.spawnPrefabs.Add(gameObject);

            NetworkClientInspector inspector = ScriptableObject.CreateInstance<NetworkClientInspector>();

            inspector.RegisterPrefabs(client);

            Assert.That(client.spawnPrefabs, Has.Count.GreaterThan(13));

            foreach (var prefab in client.spawnPrefabs)
            {
                Assert.That(prefab.GetComponent<NetworkIdentity>(), Is.Not.Null);
            }

            Assert.That(client.spawnPrefabs, Contains.Item(gameObject));
        }        
    }
}
