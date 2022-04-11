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
        GameObject playerReplacement;

        [Test]
        public void OnSpawnAssetSceneIDFailureExceptionTest()
        {
            var msg = new SpawnMessage();
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                clientObjectManager.OnSpawn(msg);
            });

            Assert.That(ex.Message, Is.EqualTo($"OnSpawn has empty prefabHash and sceneId for netId: {msg.netId}"));
        }

        [UnityTest]
        public IEnumerator GetPrefabTest() => UniTask.ToCoroutine(async () =>
        {
            int hash = NewUniqueHash();
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            NetworkIdentity identity = prefabObject.GetComponent<NetworkIdentity>();

            clientObjectManager.RegisterPrefab(identity, hash);

            await UniTask.Delay(1);

            NetworkIdentity result = clientObjectManager.GetPrefab(hash);

            Assert.That(result, Is.SameAs(identity));

            Object.Destroy(prefabObject);
        });

        [Test]
        public void RegisterPrefabDelegateEmptyIdentityExceptionTest()
        {
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            NetworkIdentity identity = prefabObject.GetComponent<NetworkIdentity>();
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
            NetworkIdentity identity = prefabObject.GetComponent<NetworkIdentity>();
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
            NetworkIdentity identity = prefabObject.GetComponent<NetworkIdentity>();
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
            NetworkIdentity identity = prefabObject.GetComponent<NetworkIdentity>();
            identity.PrefabHash = NewUniqueHash();

            clientObjectManager.RegisterPrefab(identity, TestSpawnDelegate, TestUnspawnDelegate);

            Assert.That(clientObjectManager.spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(clientObjectManager.unspawnHandlers.ContainsKey(identity.PrefabHash));

            clientObjectManager.UnregisterSpawnHandler(identity.PrefabHash);

            Assert.That(!clientObjectManager.spawnHandlers.ContainsKey(identity.PrefabHash));
            Assert.That(!clientObjectManager.unspawnHandlers.ContainsKey(identity.PrefabHash));

            Object.Destroy(prefabObject);
        }

        NetworkIdentity TestSpawnDelegate(SpawnMessage msg)
        {
            return new GameObject("spawned", typeof(NetworkIdentity)).GetComponent<NetworkIdentity>();
        }

        void TestUnspawnDelegate(NetworkIdentity identity)
        {
            Object.Destroy(identity.gameObject);
        }

        [Test]
        public void GetPrefabEmptyNullTest()
        {
            NetworkIdentity result = clientObjectManager.GetPrefab(0);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetPrefabNotFoundNullTest()
        {
            NetworkIdentity result = clientObjectManager.GetPrefab(NewUniqueHash());

            Assert.That(result, Is.Null);
        }

        //Used to ensure the test has a unique non empty guid
        int NewUniqueHash()
        {
            int testGuid = Guid.NewGuid().GetHashCode();

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
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.PrefabHash = NewUniqueHash();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverObjectManager.ReplaceCharacter(server.LocalPlayer, playerReplacement, true);

            Assert.That(server.LocalClient.Player.Identity, Is.EqualTo(replacementIdentity));
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
            int hash = NewUniqueHash();
            var prefabObject = new GameObject("prefab", typeof(NetworkIdentity));
            NetworkIdentity identity = prefabObject.GetComponent<NetworkIdentity>();
            identity.PrefabHash = hash;
            ulong sceneId = 10ul;
            clientObjectManager.spawnableObjects.Add(sceneId, identity);

            NetworkIdentity result = clientObjectManager.SpawnSceneObject(new SpawnMessage { sceneId = sceneId, prefabHash = hash });

            Assert.That(result, Is.SameAs(identity));

            Object.Destroy(prefabObject);
        }
    }
}
