using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.ClientServer
{
    [TestFixture]
    public class ClientObjectManagerTest : ClientServerSetup<MockComponent>
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
            var identity = CreateNetworkIdentity();

            clientObjectManager.RegisterPrefab(identity, hash);

            await UniTask.Delay(1);

            var result = clientObjectManager.GetPrefab(hash);

            Assert.That(result, Is.SameAs(identity));
        });

        [Test]
        public void RegisterPrefabDelegateEmptyIdentityExceptionTest()
        {
            var identity = CreateNetworkIdentity();

            identity.PrefabHash = 0;

            Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);
            });
        }

        [Test]
        public void RegisterPrefabDelegateTest()
        {
            var identity = CreateNetworkIdentity();

            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.That(clientObjectManager._spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(clientObjectManager._unspawnHandlers.ContainsKey(identity.PrefabHash));
        }

        [Test]
        public void UnregisterPrefabTest()
        {
            var identity = CreateNetworkIdentity();

            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.That(clientObjectManager._spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(clientObjectManager._unspawnHandlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterPrefab(identity);

            Assert.That(!clientObjectManager._spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(!clientObjectManager._unspawnHandlers.ContainsKey(identity.PrefabHash));
        }

        [Test]
        public void UnregisterSpawnHandlerTest()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.That(clientObjectManager._spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(clientObjectManager._unspawnHandlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterSpawnHandler(identity.PrefabHash);

            Assert.That(!clientObjectManager._spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(!clientObjectManager._unspawnHandlers.ContainsKey(identity.PrefabHash));
        }

        private NetworkIdentity TestSpawnDelegate(SpawnMessage msg)
        {
            return CreateNetworkIdentity();
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

            if (clientObjectManager._prefabs.ContainsKey(testGuid))
            {
                testGuid = NewUniqueHash();
            }
            return testGuid;
        }

        [UnityTest]
        public IEnumerator ObjectHideTest() => UniTask.ToCoroutine(async () =>
        {
            clientObjectManager.OnObjectHide(new ObjectHideMessage
            {
                netId = clientIdentity.NetId
            });

            await AsyncUtil.WaitUntilWithTimeout(() => clientIdentity == null);

            Assert.That(clientIdentity == null);
        });

        [UnityTest]
        public IEnumerator ObjectDestroyTest() => UniTask.ToCoroutine(async () =>
        {
            clientObjectManager.OnObjectDestroy(new ObjectDestroyMessage
            {
                netId = clientIdentity.NetId
            });

            await AsyncUtil.WaitUntilWithTimeout(() => clientIdentity == null);

            Assert.That(clientIdentity == null);
        });

        [Test]
        public void SpawnSceneObjectTest()
        {
            //Setup new scene object for test
            var hash = NewUniqueHash();
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = hash;
            var sceneId = 10ul;
            clientObjectManager.spawnableObjects.Add(sceneId, identity);

            var result = clientObjectManager.SpawnSceneObject(new SpawnMessage { sceneId = sceneId, prefabHash = hash });

            Assert.That(result, Is.SameAs(identity));
        }
    }
}
