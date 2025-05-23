using System;
using System.Collections;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.ClientServer
{
    [TestFixture]
    public class ClientObjectManagerTest : ClientServerSetup
    {
        [Test]
        public void OnSpawnAssetSceneIDFailureExceptionTest()
        {
            var msg = new SpawnMessage();
            var ex = Assert.Throws<SpawnObjectException>(() =>
            {
                clientObjectManager.OnSpawn(msg);
            });

            Assert.That(ex.Message, Is.EqualTo($"Empty prefabHash and sceneId for netId: {msg.NetId}"));
        }

        [UnityTest]
        public IEnumerator GetPrefabTest() => UniTask.ToCoroutine(async () =>
        {
            var hash = NewUniqueHash();
            var identity = CreateNetworkIdentity();

            clientObjectManager.RegisterPrefab(identity, hash);

            await UniTask.Delay(1);

            var handler = clientObjectManager.GetSpawnHandler(hash);

            Assert.That(handler.Prefab, Is.SameAs(identity));
        });

        [Test]
        public void ThrowsIfPrefabHas0Hash()
        {
            var identity = CreateNetworkIdentity();

            identity.Editor_PrefabHash = 0;

            var actual = Assert.Throws<ArgumentException>(() =>
              {
                  clientObjectManager.RegisterSpawnHandler(identity, TestSpawnDelegate, TestUnspawnDelegate);
              });

            var expected = new ArgumentException($"prefabHash is zero on {identity.name}", "identity");
            Assert.That(actual, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void RegisterPrefabDelegate()
        {
            var identity = CreateNetworkIdentity();

            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
            var handlers = clientObjectManager._handlers[identity.PrefabHash];
            Assert.That(handlers.Prefab == identity, "prefab should also be added to handler if it is passed to regiester so that user can use it if they want to");
            Assert.That(handlers.Handler == TestSpawnDelegate);
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
            Assert.That(handlers.Prefab == identity, "prefab should also be added to handler if it is passed to regiester so that user can use it if they want to");
            Assert.That(handlers.Handler == TestSpawnDelegate);
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

            Assert.DoesNotThrow(() =>
            {
                clientObjectManager.RegisterSpawnHandler(identity, (msg) => null, (obj) => { });
            }, "should not throw if adding handler to prefab that ia already Register");
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
            Assert.That(handler.Handler == null);
            Assert.That(handler.UnspawnHandler == null);

            clientObjectManager.RegisterUnspawnHandler(identity, TestUnspawnDelegate);
            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
            handler = clientObjectManager._handlers[identity.PrefabHash];
            Assert.That(handler.Prefab == identity);
            Assert.That(handler.Handler == null);
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
        private NetworkIdentity TestSpawnDelegateWithMock(SpawnMessage msg)
        {
            var identity = CreateNetworkIdentity();
            return identity;
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
                var handler = clientObjectManager.GetSpawnHandler(0);
            });

            var expected = new ArgumentException("prefabHash is zero", "prefabHash");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowsWhenGetPrefabNotFound()
        {
            var prefabHash = NewUniqueHash();
            var exception = Assert.Throws<SpawnObjectException>(() =>
            {
                var handler = clientObjectManager.GetSpawnHandler(prefabHash);
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
                NetId = NET_ID,
                PrefabHash = identity.PrefabHash,
                Payload = new ArraySegment<byte>(new byte[0]),
                SpawnValues = new SpawnValues { Position = pos, }
            };

            clientObjectManager.OnSpawn(msg);
            var world = client.World;
            // does not exist yet
            Assert.IsFalse(world.TryGetIdentity(NET_ID, out var _));

            Assert.That(handlerStarted, Is.EqualTo(1));
            Assert.That(handlerFinished, Is.EqualTo(0));

            // pending checks
            Assert.IsTrue(clientObjectManager.pendingSpawn.ContainsKey(NET_ID));
            var pending = clientObjectManager.pendingSpawn[NET_ID];
            Assert.That(pending.NetId, Is.EqualTo(NET_ID));
            Assert.That(pending.PendingCount, Is.EqualTo(0), "no pending for first message only");

            // wait for delay+1 frame
            await UniTask.Delay(DELAY);
            await UniTask.Yield();

            Assert.That(handlerStarted, Is.EqualTo(1));
            Assert.That(handlerFinished, Is.EqualTo(1));
            Assert.IsTrue(world.TryGetIdentity(NET_ID, out var spawned));
            Assert.That(spawned.transform.position, Is.EqualTo(pos));
        });

        [UnityTest]
        public IEnumerator SpawnsAsyncPending() => UniTask.ToCoroutine(async () =>
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
                NetId = NET_ID,
                PrefabHash = identity.PrefabHash,
                Payload = new ArraySegment<byte>(new byte[0]),
                SpawnValues = new SpawnValues { Position = pos, }
            };

            clientObjectManager.OnSpawn(msg);
            var world = client.World;
            // does not exist yet
            Assert.IsFalse(world.TryGetIdentity(NET_ID, out var _));

            Assert.That(handlerStarted, Is.EqualTo(1));
            Assert.That(handlerFinished, Is.EqualTo(0));

            // pending checks
            Assert.IsTrue(clientObjectManager.pendingSpawn.ContainsKey(NET_ID));
            var pending = clientObjectManager.pendingSpawn[NET_ID];
            Assert.That(pending.NetId, Is.EqualTo(NET_ID));
            Assert.That(pending.PendingCount, Is.EqualTo(0), "no pending for first message only");

            // send 2nd message, it should be added to pending
            var pos2 = new Vector3(120, 0, 0);
            var msg2 = new SpawnMessage
            {
                NetId = NET_ID,
                PrefabHash = identity.PrefabHash,
                Payload = new ArraySegment<byte>(new byte[0]),
                SpawnValues = new SpawnValues { Position = pos2, }
            };
            clientObjectManager.OnSpawn(msg2);
            Assert.IsTrue(clientObjectManager.pendingSpawn.ContainsKey(NET_ID));
            Assert.That(clientObjectManager.pendingSpawn[NET_ID], Is.EqualTo(pending), "pending instance should not change");
            Assert.That(pending.PendingCount, Is.EqualTo(1), "Should have pending for 2nd message");

            var pos3 = new Vector3(150, 0, 0);
            var msg3 = new SpawnMessage
            {
                NetId = NET_ID,
                PrefabHash = identity.PrefabHash,
                Payload = new ArraySegment<byte>(new byte[0]),
                SpawnValues = new SpawnValues { Position = pos3, }
            };
            clientObjectManager.OnSpawn(msg3);

            Assert.IsTrue(clientObjectManager.pendingSpawn.ContainsKey(NET_ID));
            Assert.That(clientObjectManager.pendingSpawn[NET_ID], Is.EqualTo(pending), "pending instance should not change");
            Assert.That(pending.PendingCount, Is.EqualTo(2), "Should have pending for 2nd message");

            // wait for delay+1 frame
            await UniTask.Delay(DELAY);
            await UniTask.Yield();

            Assert.That(handlerStarted, Is.EqualTo(1));
            Assert.That(handlerFinished, Is.EqualTo(1));
            Assert.IsTrue(world.TryGetIdentity(NET_ID, out var spawned));
            Assert.That(spawned.transform.position, Is.EqualTo(pos3), "Should apply extra message, in order");
            Assert.IsFalse(clientObjectManager.pendingSpawn.ContainsKey(NET_ID), "Key should have been removed");
        });


        [Test]
        public void ThrowsWhenAddingAsyncWhenAlreadyAdded()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, (msg) => null, (obj) => { });

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var asyncHandler = new SpawnHandlerAsyncDelegate((msg) => UniTask.FromResult<NetworkIdentity>(null));
                clientObjectManager.RegisterSpawnHandler(identity.PrefabHash, asyncHandler, (obj) => { });
            });

            Assert.That(exception, Has.Message.EqualTo($"Handlers with hash {identity.PrefabHash:X} already registered. " +
                    $"Unregister before adding new or prefabshandlers. Too add Unspawn handler to prefab use RegisterUnspawnHandler instead"));

        }

        [Test]
        public void CanAddDynamicHandler()
        {
            var del = new DynamicSpawnHandlerDelegate(DynamicHandler);
            clientObjectManager.RegisterDynamicSpawnHandler(del);

            Assert.That(clientObjectManager._dynamicHandlers.Contains(del));
        }

        [Test]
        public void ThrowsIfDynamicHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                clientObjectManager.RegisterDynamicSpawnHandler(default(DynamicSpawnHandlerDelegate));
            });
        }

        [Test]
        public void DynamicHandlerCanReturnNull()
        {
            // register empty dynamic and prefab
            clientObjectManager.RegisterDynamicSpawnHandler(EmptyDynamicHandler);
            var prefab = CreateNetworkIdentity();
            prefab.PrefabHash = NewUniqueHash();
            clientObjectManager.RegisterPrefab(prefab);

            // then get handler for that prefab

            var handler = clientObjectManager.GetSpawnHandler(prefab.PrefabHash);
            Assert.That(handler.Prefab, Is.EqualTo(prefab));
        }

        [Test]
        public void ThrowsIfNoHandlerInDynamicOrOther()
        {
            // register empty dynamic and prefab
            clientObjectManager.RegisterDynamicSpawnHandler(EmptyDynamicHandler);
            var prefab = CreateNetworkIdentity();
            prefab.PrefabHash = NewUniqueHash();
            clientObjectManager.RegisterPrefab(prefab);

            // then get handler for that prefab

            var noHandlerHash = NewUniqueHash();

            var actual = Assert.Throws<SpawnObjectException>(() =>
            {
                var handler = clientObjectManager.GetSpawnHandler(noHandlerHash);

            });
            var expected = new SpawnObjectException($"No prefab for {noHandlerHash:X}. did you forget to add it to the ClientObjectManager?");
            Assert.That(actual, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void UsesHandlerFromDynamic()
        {
            // register empty dynamic and prefab
            clientObjectManager.RegisterDynamicSpawnHandler(DynamicHandler);

            // then get handler for that prefab
            var handler100 = clientObjectManager.GetSpawnHandler(100);
            var handler101 = clientObjectManager.GetSpawnHandler(101);

            Assert.That(handler100.Handler.GetMethodInfo().Name, Is.EqualTo(nameof(TestSpawnDelegate)));
            Assert.That(handler101.Handler.GetMethodInfo().Name, Is.EqualTo(nameof(TestSpawnDelegateWithMock)));
        }

        [Test]
        public void UsesHandlerBeforeDynamicHandler()
        {
            // register empty dynamic and prefab
            clientObjectManager.RegisterDynamicSpawnHandler(DynamicHandler);
            var prefab = CreateNetworkIdentity();
            prefab.PrefabHash = 100;
            clientObjectManager.RegisterPrefab(prefab);


            // then get handler for that prefab
            var handler100 = clientObjectManager.GetSpawnHandler(100);
            var handler101 = clientObjectManager.GetSpawnHandler(101);

            Assert.That(handler100.Handler, Is.Null);
            Assert.That(handler100.Prefab, Is.EqualTo(prefab));
            Assert.That(handler101.Handler.GetMethodInfo().Name, Is.EqualTo(nameof(TestSpawnDelegateWithMock)));
        }


        private SpawnHandler EmptyDynamicHandler(int prefabHash)
        {
            return null;
        }

        private SpawnHandler DynamicHandler(int prefabHash)
        {
            if (prefabHash == 100)
                return new SpawnHandler(TestSpawnDelegate, null);
            else if (prefabHash == 101)
                return new SpawnHandler(TestSpawnDelegateWithMock, null);
            else
                return null;
        }

        //Used to ensure the test has a unique non empty guid
        private int NewUniqueHash()
        {
            var testGuid = Guid.NewGuid().GetHashCode();

            // recursive, so dont need while
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
                NetId = clientIdentity.NetId
            });

            await AsyncUtil.WaitUntilWithTimeout(() => clientIdentity == null);

            Assert.That(clientIdentity == null);
        });

        [UnityTest]
        public IEnumerator ObjectDestroyTest() => UniTask.ToCoroutine(async () =>
        {
            clientObjectManager.OnObjectDestroy(new ObjectDestroyMessage
            {
                NetId = clientIdentity.NetId
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

            var result = clientObjectManager.SpawnSceneObject(new SpawnMessage { SceneId = sceneId, PrefabHash = hash });

            Assert.That(result, Is.SameAs(identity));
        }
    }
}
