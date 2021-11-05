using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    [TestFixture]
    public class ServerObjectManagerTests : ClientServerSetup<MockComponent>
    {
        void AssertNoIdentityMessage(InvalidOperationException ex, string name) => Assert.That(ex.Message, Is.EqualTo($"Gameobject {name} doesn't have NetworkIdentity."));
        void AssertNoIdentityMessage(InvalidOperationException ex) => AssertNoIdentityMessage(ex, new GameObject().name);

        NetworkIdentity CreatePlayerReplacement()
        {
            var playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            toDestroy.Add(playerReplacement);
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.PrefabHash = Guid.NewGuid().GetHashCode();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            return replacementIdentity;
        }

        [Test]
        public void ThrowsIfSpawnCalledWhenServerIsNotAcctive()
        {
            var obj = new GameObject();

            server.Stop();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject().AddComponent<NetworkIdentity>(), serverPlayer);
            });

            Assert.That(ex.Message, Is.EqualTo("NetworkServer is not active. Cannot spawn objects without an active server."));
            GameObject.DestroyImmediate(obj);
        }

        [Test]
        public void ThrowsIfSpawnCalledOwnerHasNoNetworkIdentity()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), new GameObject());
            });

            AssertNoIdentityMessage(ex);
        }

        [UnityTest]
        public IEnumerator SpawnByIdentityTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.Spawn(serverIdentity);

            await AsyncUtil.WaitUntilWithTimeout(() => (NetworkServer)serverIdentity.Server == server);
        });

        [Test]
        public void ThrowsIfSpawnCalledWithOwnerWithNoOwnerTest()
        {
            var badOwner = new GameObject();
            badOwner.AddComponent<NetworkIdentity>();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), badOwner);
            });

            Assert.That(ex.Message, Is.EqualTo("Player object is not a player in the connection"));
            GameObject.DestroyImmediate(badOwner);
        }

        [UnityTest]
        public IEnumerator ShowForPlayerTest() => UniTask.ToCoroutine(async () =>
        {
            bool invoked = false;

            ClientMessageHandler.RegisterHandler<SpawnMessage>(msg => invoked = true);

            serverPlayer.SceneIsReady = true;

            // call ShowForConnection
            serverObjectManager.ShowToPlayer(serverIdentity, serverPlayer);

            // todo assert correct message was sent using Substitute for socket or player

            await AsyncUtil.WaitUntilWithTimeout(() => invoked);
        });

        [Test]
        public void SpawnSceneObject()
        {
            NetworkIdentity sceneObject = InstantiateForTest(playerPrefab).GetComponent<NetworkIdentity>();
            sceneObject.SetSceneId(42);

            Debug.Assert(sceneObject.NetId == 0, "Identity should be unspawned for this test");
            serverObjectManager.SpawnObjects();
            Assert.That(sceneObject.NetId, Is.Not.Zero);
        }

        [Test]
        public void DoesNotSpawnNonSceneObject()
        {
            NetworkIdentity sceneObject = InstantiateForTest(playerPrefab).GetComponent<NetworkIdentity>();
            sceneObject.SetSceneId(0);

            Debug.Assert(sceneObject.NetId == 0, "Identity should be unspawned for this test");
            serverObjectManager.SpawnObjects();
            Assert.That(sceneObject.NetId, Is.Zero);
        }

        [Test]
        public void SpawnEvent()
        {
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            server.World.onSpawn += mockHandler;
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        }

        [UnityTest]
        public IEnumerator ClientSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            client.World.onSpawn += mockHandler;
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        });

        [UnityTest]
        public IEnumerator ClientUnSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            client.World.onUnspawn += mockHandler;
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);
            serverObjectManager.Destroy(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));
            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
        });

        [Test]
        public void UnSpawnEvent()
        {
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            server.World.onUnspawn += mockHandler;
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);
            serverObjectManager.Destroy(newObj);
            mockHandler.Received().Invoke(newObj.GetComponent<NetworkIdentity>());
        }

        [Test]
        public void ReplacePlayerBaseTest()
        {
            NetworkIdentity replacement = CreatePlayerReplacement();

            serverObjectManager.ReplaceCharacter(serverPlayer, replacement);

            Assert.That(serverPlayer.Identity, Is.EqualTo(replacement));
        }

        [Test]
        public void ReplacePlayerDontKeepAuthTest()
        {
            NetworkIdentity replacement = CreatePlayerReplacement();

            serverObjectManager.ReplaceCharacter(serverPlayer, replacement, true);

            Assert.That(clientIdentity.Owner, Is.EqualTo(null));
        }

        [Test]
        public void ReplacePlayerPrefabHashTest()
        {
            NetworkIdentity replacement = CreatePlayerReplacement();
            int hash = replacement.PrefabHash;

            serverObjectManager.ReplaceCharacter(serverPlayer, replacement, hash);

            Assert.That(serverPlayer.Identity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void AddPlayerForConnectionPrefabHashTest()
        {
            NetworkIdentity replacement = CreatePlayerReplacement();
            int hash = replacement.PrefabHash;

            serverPlayer.Identity = null;

            serverObjectManager.AddCharacter(serverPlayer, replacement, hash);

            Assert.That(replacement == serverPlayer.Identity);
        }

        [UnityTest]
        public IEnumerator DestroyCharacter() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.DestroyCharacter(serverPlayer);

            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayerGO == null);
            Assert.That(clientPlayerGO == null);
        });

        [UnityTest]
        public IEnumerator DestroyCharacterKeepServerObject() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.DestroyCharacter(serverPlayer, destroyServerObject: false);

            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayerGO != null);
            Assert.That(clientPlayerGO == null);
        });

        [UnityTest]
        public IEnumerator RemoveCharacterKeepAuthority() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemoveCharacter(serverPlayer, true);

            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayerGO != null);
            Assert.That(serverIdentity.Owner == serverPlayer);

            Assert.That(clientPlayerGO != null);
            Assert.That(clientIdentity.HasAuthority, Is.True);
            Assert.That(clientIdentity.IsLocalPlayer, Is.False);
        });

        [UnityTest]
        public IEnumerator RemoveCharacter() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemoveCharacter(serverPlayer);

            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayerGO != null);
            Assert.That(serverIdentity.Owner == null);

            Assert.That(clientPlayerGO != null);
            Assert.That(clientIdentity.HasAuthority, Is.False);
            Assert.That(clientIdentity.IsLocalPlayer, Is.False);
        });

        [Test]
        public void DestroyCharacterThrowsIfNoCharacter()
        {
            INetworkPlayer player = Substitute.For<INetworkPlayer>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.DestroyCharacter(player);
            });
        }

        [Test]
        public void RemoveCharacterThrowsIfNoCharacter()
        {
            INetworkPlayer player = Substitute.For<INetworkPlayer>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.RemoveCharacter(player, false);
            });
        }

        [Test]
        public void ThrowsIfSpawnedCalledWithoutANetworkIdentity()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), clientPlayer);
            });

            AssertNoIdentityMessage(ex);
        }


        [Test]
        public void AddCharacterNoIdentityExceptionTest()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.AddCharacter(serverPlayer, new GameObject());
            });
            AssertNoIdentityMessage(ex);

        }

        [Test]
        public void ReplacePlayerNoIdentityExceptionTest()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.ReplaceCharacter(serverPlayer, new GameObject(), true);
            });
            AssertNoIdentityMessage(ex);
        }

        [UnityTest]
        public IEnumerator SpawnObjectsExceptionTest() => UniTask.ToCoroutine(async () =>
        {
            server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.SpawnObjects();
            });

            Assert.That(exception, Has.Message.EqualTo("Server was not active"));
        });
    }
}

