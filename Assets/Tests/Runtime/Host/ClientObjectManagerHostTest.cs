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

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
            var handler = clientObjectManager._handlers[identity.PrefabHash];
            Assert.That(handler.Prefab == null);
            Assert.That(handler.SpawnHandler == TestSpawnDelegate);
            Assert.That(handler.UnspawnHandler == TestUnspawnDelegate);
        }

        [Test]
        public void UnregisterPrefabTest()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.IsTrue(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterPrefab(identity);

            Assert.IsFalse(clientObjectManager._handlers.ContainsKey(identity.PrefabHash));
        }

        [Test]
        public void UnregisterSpawnHandlerTest()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

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

        [Test]
        public void ReplacePlayerHostTest()
        {
            var replacementIdentity = CreateNetworkIdentity();
            replacementIdentity.PrefabHash = NewUniqueHash();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverObjectManager.ReplaceCharacter(server.LocalPlayer, replacementIdentity.gameObject, true);

            Assert.That(server.LocalClient.Player.Identity, Is.EqualTo(replacementIdentity));
        }

        [Test]
        public void UnSpawnShouldAssertIfCalledInHostMode()
        {
            LogAssert.Expect(LogType.Assert, "UnSpawn should not be called in host mode");
            clientObjectManager.OnObjectDestroy(new ObjectDestroyMessage
            {
                netId = playerIdentity.NetId
            });
        }

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
