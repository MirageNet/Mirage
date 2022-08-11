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
        [Test]
        public void OnSpawnAssetSceneIDFailureExceptionTest()
        {
            var msg = new SpawnMessage();
            var ex = Assert.Throws<SpawnObjectException>(() =>
            {
                clientObjectManager.OnSpawn(msg);
            });

            Assert.That(ex.Message, Is.EqualTo($"Empty prefabHash and sceneId for netId: {msg.netId}"));
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
        public void ThrowsIfPrefabHas0Hash()
        {
            var identity = CreateNetworkIdentity();

            identity.PrefabHash = 0;

            Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterSpawnHandler(identity, TestSpawnDelegate, TestUnspawnDelegate);
            });
        }

        [Test]
        public void RegisterPrefabDelegate()
        {
            var identity = CreateNetworkIdentity();

            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
            var handlers = clientObjectManager._handlers[identity.PrefabHash];
            Assert.That(handlers.Prefab == null, "should not have added prefab with handlers");
            Assert.That(handlers.SpawnHandler == TestSpawnDelegate);
            Assert.That(handlers.UnspawnHandler == TestUnspawnDelegate);
        }

        [Test]
        public void IsAllowedToGiveNullUnspawn()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity, TestSpawnDelegate, null);

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
            var handlers = clientObjectManager._handlers[identity.PrefabHash];
            Assert.That(handlers.Prefab == null, "should not have added prefab with handlers");
            Assert.That(handlers.SpawnHandler == TestSpawnDelegate);
            Assert.That(handlers.UnspawnHandler == null);
        }

        [Test]
        public void ThrowsIfSpawnHandlerNull()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            Assert.Throws<ArgumentNullException>(() =>
            {
                clientObjectManager.RegisterSpawnHandler(identity, (SpawnHandlerDelegate)null, TestUnspawnDelegate);
            });
        }

        [Test]
        public void ThrowsIfPrefabAlreadyRegistered()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity);

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterSpawnHandler(identity, (msg) => null, (obj) => { });
            });

            Assert.That(exception, Has.Message.EqualTo($"Prefab with hash {identity.PrefabHash:X} already registered. " +
                    $"Unregister before adding new or prefabshandlers. Too add Unspawn handler to prefab use RegisterUnspawnHandler instead"));
        }

        [Test]
        public void ThrowsIfPrefabAlreadyRegisteredAsHandles()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity);

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, (msg) => null, (obj) => { });
            });

            Assert.That(exception, Has.Message.EqualTo($"Prefab with hash {identity.PrefabHash:X} already registered. " +
                    $"Unregister before adding new or prefabshandlers. Too add Unspawn handler to prefab use RegisterUnspawnHandler instead"));
        }

        [Test]
        public void ThrowsIfHandlerAlreadyRegistered()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, (msg) => null, (obj) => { });

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterPrefab(identity);
            });

            Assert.That(exception, Has.Message.EqualTo($"Handlers with hash {identity.PrefabHash:X} already registered. " +
                    $"Unregister before adding new or prefabshandlers. Too add Unspawn handler to prefab use RegisterUnspawnHandler instead"));
        }

        [Test]
        public void CanRemovePrefab()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity);

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterPrefab(identity);

            // check was removed
            Assert.IsFalse(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
        }

        [Test]
        public void CanRemoveHandler()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, (msg) => null, (obj) => { });

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterSpawnHandler(identity.PrefabHash);

            // check was removed
            Assert.IsFalse(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
        }

        [Test]
        public void CanAddUnspawnToPrefab()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity);
            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
            var handler = clientObjectManager._handlers[identity.PrefabHash];
            Assert.That(handler.Prefab == identity);
            Assert.That(handler.SpawnHandler == null);
            Assert.That(handler.UnspawnHandler == null);

            clientObjectManager.RegisterUnspawnHandler(identity, TestUnspawnDelegate);
            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
            handler = clientObjectManager._handlers[identity.PrefabHash];
            Assert.That(handler.Prefab == identity);
            Assert.That(handler.SpawnHandler == null);
            Assert.That(handler.UnspawnHandler == TestUnspawnDelegate);
        }

        [Test]
        public void UnregisterPrefab()
        {
            var identity = CreateNetworkIdentity();

            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterPrefab(identity);

            Assert.IsFalse(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
        }

        [Test]
        public void UnregisterSpawnHandler()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterSpawnHandler(identity.PrefabHash);

            Assert.IsFalse(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
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
        public void ThrwosWhenGetPrefabIsGivenZero()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                var result = clientObjectManager.GetPrefab(0);
            });

            var expected = new ArgumentException("prefabHash was 0", "prefabHash");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowsWhenGetPrefabNotFound()
        {
            var prefabHash = NewUniqueHash();
            var exception = Assert.Throws<SpawnObjectException>(() =>
            {
                var result = clientObjectManager.GetPrefab(prefabHash);
            });

            Assert.That(exception, Has.Message.EqualTo($"No prefab for {prefabHash:X}. did you forget to add it to the ClientObjectManager?"));
        }

        [UnityTest]
        public IEnumerator SpawnsAsync() => UniTask.ToCoroutine(async () =>
        {
            const int NET_ID = 1000;
            const int DELAY = 200;
            var pos = new Vector3(100, 0, 0);
            var handlerStarted = 0;
            var handlerFinished = 0;

            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();
            clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, new SpawnHandlerAsyncDelegate(async (msg) =>
            {
                handlerStarted++;
                await UniTask.Delay(DELAY);
                handlerFinished++;
                return identity;
            }), (obj) => { });

            var msg = new SpawnMessage
            {
                netId = NET_ID,
                prefabHash = identity.PrefabHash,
                payload = new ArraySegment<byte>(new byte[0]),
                position = pos,
            };

            clientObjectManager.OnSpawn(msg);
            var world = client.World;
            // does not exist yet
            Assert.IsFalse(world.TryGetIdentity(NET_ID, out var _));

            Assert.That(handlerStarted, Is.EqualTo(1));
            Assert.That(handlerFinished, Is.EqualTo(0));

            // wait for delay+1 frame
            await UniTask.Delay(DELAY);
            await UniTask.Yield();

            Assert.That(handlerStarted, Is.EqualTo(1));
            Assert.That(handlerFinished, Is.EqualTo(1));
            Assert.IsTrue(world.TryGetIdentity(NET_ID, out var spawned));
            Assert.That(spawned.transform.position, Is.EqualTo(pos));
        });

        [Test]
        public void ThrowsWhenAddingAsyncWhenAlreadyAdded()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, (msg) => null, (obj) => { });

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, new SpawnHandlerAsyncDelegate(async (msg) => null), (obj) => { });
            });

            Assert.That(exception, Has.Message.EqualTo($"Handlers with hash {identity.PrefabHash:X} already registered. " +
                    $"Unregister before adding new or prefabshandlers. Too add Unspawn handler to prefab use RegisterUnspawnHandler instead"));

        }

        //Used to ensure the test has a unique non empty guid
        private int NewUniqueHash()
        {
            var testGuid = Guid.NewGuid().GetHashCode();

            if (clientObjectManager._handlers.ContainsKey(testGuid))
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
