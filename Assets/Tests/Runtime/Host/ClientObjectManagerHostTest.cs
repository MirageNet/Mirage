using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public class ClientObjectManagerHostTest : HostSetup<MockComponent>
    {
        private GameObject playerReplacement;

        [Test]
        public void OnSpawnAssetSceneIDFailureExceptionTest()
        {
            var msg = new SpawnMessage();
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.OnSpawn(msg);
            });

            Assert.That(ex.Message, Is.EqualTo($"OnSpawn has empty prefabHash and sceneId for netId: {msg.netId}"));
        }

        [UnityTest]
        public IEnumerator GetPrefabTest() => UniTask.ToCoroutine(async () =>
        {
            var hash = NewUniqueHash();
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            var identity = prefabObject.GetComponent<NetworkIdentity>();

            clientObjectManager.RegisterPrefab(identity, hash);

            await UniTask.Delay(1);

            var result = clientObjectManager.GetPrefab(hash);

            Assert.That(result, Is.SameAs(identity));

            Object.Destroy(prefabObject);
        });

        [Test]
        public void RegisterPrefabDelegateEmptyIdentityExceptionTest()
        {
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            var identity = prefabObject.GetComponent<NetworkIdentity>();
            identity.PrefabHash = 0;

            Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);
            });

            Object.Destroy(prefabObject);
        }

        [Test]
        public void RegisterPrefabDelegateTest()
        {
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            var identity = prefabObject.GetComponent<NetworkIdentity>();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.That(clientObjectManager.spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(clientObjectManager.unspawnHandlers.ContainsKey(identity.PrefabHash));

            Object.Destroy(prefabObject);
        }

        [Test]
        public void UnregisterPrefabTest()
        {
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            var identity = prefabObject.GetComponent<NetworkIdentity>();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.That(clientObjectManager.spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(clientObjectManager.unspawnHandlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterPrefab(identity);

            Assert.That(!clientObjectManager.spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(!clientObjectManager.unspawnHandlers.ContainsKey(identity.PrefabHash));

            Object.Destroy(prefabObject);
        }

        [Test]
        public void UnregisterSpawnHandlerTest()
        {
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            var identity = prefabObject.GetComponent<NetworkIdentity>();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.That(clientObjectManager.spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(clientObjectManager.unspawnHandlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterSpawnHandler(identity.PrefabHash);

            Assert.That(!clientObjectManager.spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(!clientObjectManager.unspawnHandlers.ContainsKey(identity.PrefabHash));

            Object.Destroy(prefabObject);
        }

        private NetworkIdentity TestSpawnDelegate(SpawnMessage msg)
        {
            return new GameObject("spawned", typeof(NetworkIdentity)).GetComponent<NetworkIdentity>();
        }

        private void TestUnspawnDelegate(NetworkIdentity identity)
        {
            Object.Destroy(identity.gameObject);
        }

        [Test]
        public void GetPrefabEmptyNullTest()
        {
            var result = clientObjectManager.GetPrefab(0);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetPrefabNotFoundNullTest()
        {
            var result = clientObjectManager.GetPrefab(NewUniqueHash());

            Assert.That(result, Is.Null);
        }

        //Used to ensure the test has a unique non empty guid
        private int NewUniqueHash()
        {
            var testGuid = Guid.NewGuid().GetHashCode();

            if (clientObjectManager.prefabs.ContainsKey(testGuid))
            {
                testGuid = NewUniqueHash();
            }
            return testGuid;
        }

        [Test]
        public void ReplacePlayerHostTest()
        {
            playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            var replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.PrefabHash = NewUniqueHash();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverObjectManager.ReplaceCharacter(server.LocalPlayer, playerReplacement, true);

            Assert.That(server.LocalClient.Player.Identity, Is.EqualTo(replacementIdentity));
        }

        [Test]
        public void UnSpawnShouldAssertIfCalledInHostMode()
        {
            LogAssert.Expect(LogType.Assert, "UnSpawn should not be called in host mode");
            clientObjectManager.OnObjectDestroy(new ObjectDestroyMessage
            {
                netId = identity.NetId
            });
        }

        [Test]
        public void SpawnSceneObjectTest()
        {
            //Setup new scene object for test
            var hash = NewUniqueHash();
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            var identity = prefabObject.GetComponent<NetworkIdentity>();
            identity.PrefabHash = hash;
            var sceneId = 10ul;
            clientObjectManager.spawnableObjects.Add(sceneId, identity);

            var result = clientObjectManager.SpawnSceneObject(new SpawnMessage { sceneId = sceneId, prefabHash = hash });

            Assert.That(result, Is.SameAs(identity));

            Object.Destroy(prefabObject);
        }
    }
}
