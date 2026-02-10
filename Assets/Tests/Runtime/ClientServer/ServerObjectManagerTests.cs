using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    /// <summary>
    /// Has even in OnSerialize, can be used to check for when spawnmessage is sent
    /// </summary>
    internal class SerializeEventBehaviour : NetworkBehaviour
    {
        public event Action<bool> OnSerializeCalled;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            OnSerializeCalled?.Invoke(initialState);
            return base.OnSerialize(writer, initialState);
        }
    }

    public class ServerObjectManagerTests : ClientServerSetup
    {
        private void AssertNoIdentityMessage(InvalidOperationException ex, string name)
        {
            Assert.That(ex.Message, Is.EqualTo($"Gameobject {name} doesn't have NetworkIdentity."));
        }


        private NetworkIdentity CreatePlayerReplacement()
        {
            var replacementIdentity = CreateNetworkIdentity();
            replacementIdentity.name = "replacement";

            replacementIdentity.PrefabHash = Guid.NewGuid().GetHashCode();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            return replacementIdentity;
        }

        [Test]
        public void ThrowsIfSpawnCalledWhenServerIsNotAcctive()
        {
            var identity = CreateNetworkIdentity();

            server.Stop();

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(identity, serverPlayer);
            });

            Assert.That(ex.Message, Is.EqualTo("NetworkServer is not active. Cannot spawn objects without an active server."));
        }

        [Test]
        public void ThrowsIfSpawnCalledOwnerHasNoNetworkIdentity()
        {
            var obj = CreateGameObject();
            var owner = CreateGameObject();

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(obj, owner);
            });

            AssertNoIdentityMessage(ex, owner.name);
        }

        [UnityTest]
        public IEnumerator SpawnByIdentityTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.Spawn(serverIdentity);

            await AsyncUtil.WaitUntilWithTimeout(() => serverIdentity.Server == server);
        });

        [Test]
        public void ThrowsIfSpawnCalledWithOwnerWithNoOwnerTest()
        {
            var badOwner = CreateNetworkIdentity();
            var go = CreateGameObject();

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(go, badOwner.gameObject);
            });

            Assert.That(ex.Message, Is.EqualTo("Player object is not a player in the connection"));
        }

        [UnityTest]
        public IEnumerator ShowForPlayerTest() => UniTask.ToCoroutine(async () =>
        {
            var invoked = false;

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
            var sceneObject = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            sceneObject.SetSceneId(42);

            Debug.Assert(!sceneObject.IsSpawned, "Identity should be unspawned for this test");
            serverObjectManager.SpawnSceneObjects();
            Assert.That(sceneObject.NetId, Is.Not.Zero);
        }

        [Test]
        public void DoesNotSpawnNonSceneObject()
        {
            var sceneObject = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            sceneObject.SetSceneId(0);

            Debug.Assert(!sceneObject.IsSpawned, "Identity should be unspawned for this test");
            serverObjectManager.SpawnSceneObjects();
            Assert.That(sceneObject.NetId, Is.Zero);
        }

        [Test]
        public void SpawnEvent()
        {
            var mockHandler = Substitute.For<Action<NetworkIdentity>>();
            server.World.onSpawn += mockHandler;
            var newObj = InstantiateForTest(_characterPrefabGo);
            serverObjectManager.Spawn(newObj);

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        }

        [UnityTest]
        public IEnumerator ClientSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            var mockHandler = Substitute.For<Action<NetworkIdentity>>();
            client.World.onSpawn += mockHandler;
            var newObj = InstantiateForTest(_characterPrefabGo);
            serverObjectManager.Spawn(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        });

        [UnityTest]
        public IEnumerator ClientUnSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            var mockHandler = Substitute.For<NetworkWorld.UnspawnHandler>();
            client.World.onUnspawn += mockHandler;
            var newObj = InstantiateForTest(_characterPrefabGo);
            serverObjectManager.Spawn(newObj);
            serverObjectManager.Destroy(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));
            mockHandler.Received().Invoke(Arg.Any<uint>(), Arg.Any<NetworkIdentity>());
        });

        [Test]
        public void UnSpawnEvent()
        {
            var mockHandler = Substitute.For<NetworkWorld.UnspawnHandler>();
            server.World.onUnspawn += mockHandler;
            var newObj = InstantiateForTest(_characterPrefabGo);
            var identity = newObj.GetComponent<NetworkIdentity>();
            serverObjectManager.Spawn(identity);
            var netId = identity.NetId;
            serverObjectManager.Destroy(identity);
            mockHandler.Received().Invoke(netId, identity);
        }

        [Test]
        public void ReplacePlayerBaseTest()
        {
            var replacement = CreatePlayerReplacement();

            serverObjectManager.ReplaceCharacter(serverPlayer, replacement);

            Assert.That(serverPlayer.Identity, Is.EqualTo(replacement));
        }

        [Test]
        public void ReplacePlayerDontKeepAuthTest()
        {
            var replacement = CreatePlayerReplacement();

            serverObjectManager.ReplaceCharacter(serverPlayer, replacement, true);

            Assert.That(clientIdentity.Owner, Is.EqualTo(null));
        }

        [Test]
        public void ReplacePlayerPrefabHashTest()
        {
            var replacement = CreatePlayerReplacement();
            var hash = replacement.PrefabHash;

            serverObjectManager.ReplaceCharacter(serverPlayer, replacement, hash);

            Assert.That(serverPlayer.Identity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void AddPlayerForConnectionPrefabHashTest()
        {
            var replacement = CreatePlayerReplacement();
            var hash = replacement.PrefabHash;

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
            var player = Substitute.For<INetworkPlayer>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.DestroyCharacter(player);
            });
        }

        [Test]
        public void RemoveCharacterThrowsIfNoCharacter()
        {
            var player = Substitute.For<INetworkPlayer>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.RemoveCharacter(player, false);
            });
        }

        [Test]
        public void ThrowsIfSpawnedCalledWithoutANetworkIdentity()
        {
            var go = CreateGameObject();
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(go, clientPlayer);
            });

            AssertNoIdentityMessage(ex, go.name);
        }


        [Test]
        public void AddCharacterNoIdentityExceptionTest()
        {
            var character = CreateGameObject();
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.AddCharacter(serverPlayer, character);
            });
            AssertNoIdentityMessage(ex, character.name);
        }

        [Test]
        public void AddingExistingCharacterOnlySendsOneSpawnMessage()
        {
            if (serverPlayer.HasCharacter)
                serverObjectManager.RemoveCharacter(serverPlayer, keepAuthority: false);

            var character = CreateBehaviour<SerializeEventBehaviour>();
            // spawn character so it already eixsts
            serverObjectManager.Spawn(character.Identity);
            var called = 0;
            character.OnSerializeCalled += (initial) =>
            {
                // check when spawn message is sent
                if (initial)
                    called++;
            };

            // remove all visiblity to pretend that player has just jonied
            serverPlayer.RemoveAllVisibleObjects();

            // add as chara
            serverObjectManager.AddCharacter(serverPlayer, character.Identity);

            Assert.That(called, Is.EqualTo(1));
        }

        [Test]
        public void ReplacePlayerNoIdentityExceptionTest()
        {
            var obj = CreateGameObject();
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.ReplaceCharacter(serverPlayer, obj, true);
            });
            AssertNoIdentityMessage(ex, obj.name);
        }

        [UnityTest]
        public IEnumerator SpawnObjectsExceptionTest() => UniTask.ToCoroutine(async () =>
        {
            server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.SpawnSceneObjects();
            });

            Assert.That(exception, Has.Message.EqualTo("Server was not active"));
        });
    }
}

