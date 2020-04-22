using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mirror.Tests
{
    [TestFixture]
    public class NetworkClientTest : HostSetup<MockComponent>
    {
        [Test]
        public void IsConnectedTest()
        {
            Assert.That(client.IsConnected);
        }

        [Test]
        public void ConnectionTest()
        {
            Assert.That(client.Connection != null);
        }

        [Test]
        public void CurrentTest()
        {
            Assert.That(NetworkClient.Current == null);
        }

        [Test]
        public void ReadyTest()
        {
            client.Ready(client.Connection);
            Assert.That(client.ready);
            Assert.That(client.Connection.IsReady);
        }

        [Test]
        public void ReadyTwiceTest()
        {
            client.Ready(client.Connection);

            Assert.Throws<InvalidOperationException>(() =>
            {
                client.Ready(client.Connection);
            });
        }

        [Test]
        public void ReadyNull()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                client.Ready(null);
            });
        }

        [UnityTest]
        public IEnumerator RemovePlayerTest()
        {
            Assert.That(client.RemovePlayer(), Is.True);
            yield return null;

            Assert.That(client.LocalPlayer == null);
        }

        [Test]
        public void RegisterPrefabExceptionTest()
        {
            var gameObject = new GameObject();
            Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterPrefab(new GameObject());
            });
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void RegisterPrefabGuidExceptionTest()
        {
            var guid = Guid.NewGuid();
            var gameObject = new GameObject();

            Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterPrefab(new GameObject(), guid);
            });
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void OnSpawnAssetSceneIDFailureExceptionTest()
        {
            var msg = new SpawnMessage();
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.OnSpawn(msg);
            });

            Assert.That(ex.Message, Is.EqualTo("OnObjSpawn netId: " + msg.netId + " has invalid asset Id"));
        }

        [Test]
        public void UnregisterPrefabExceptionTest()
        {
            var gameObject = new GameObject();
            Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.UnregisterPrefab(new GameObject());
            });
            Object.DestroyImmediate(gameObject);
        }

        [UnityTest]
        public IEnumerator GetPrefabTest()
        {
            var guid = Guid.NewGuid();

            clientObjectManager.RegisterPrefab(playerGO, guid);

            yield return null;

            clientObjectManager.GetPrefab(guid, out GameObject result);

            Assert.That(result, Is.SameAs(playerGO));

            Object.Destroy(playerGO);
        }
    }
}
