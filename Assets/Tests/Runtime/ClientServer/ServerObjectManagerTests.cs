using System;
using System.Collections;
using System.Collections.Generic;
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
        public void SpawnSceneObjectsInSceneIdOrder()
        {
            // created out of order so the test can't pass from FindObjectsOfTypeAll returning creation order (or reverse)
            // hash is set so that test fails if only the lower 32 bits are compared
            var ids = new[] { (id: 30, hash: 1), (id: 10, hash: 2), (id: 50, hash: 1), (id: 20, hash: 2), (id: 40, hash: 1) };
            var sceneObjects = new NetworkIdentity[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                var sceneObject = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
                sceneObject.SetSceneId(ids[i].id, ids[i].hash);
                sceneObjects[i] = sceneObject;
            }

            serverObjectManager.SpawnSceneObjects();

            var sorted = sceneObjects.OrderBy(x => x.SceneId).ToArray();
            for (var i = 1; i < sorted.Length; i++)
                Assert.That(sorted[i].NetId, Is.GreaterThan(sorted[i - 1].NetId));
        }

        [Test]
        public void SpawnSceneObjectsKeepsNetIdOfSpawnedObject()
        {
            var spawned = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            spawned.SetSceneId(20);
            serverObjectManager.Spawn(spawned);
            var spawnedNetId = spawned.NetId;

            var unspawned = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            unspawned.SetSceneId(10);

            serverObjectManager.SpawnSceneObjects();

            Assert.That(spawned.NetId, Is.EqualTo(spawnedNetId));
            Assert.That(unspawned.NetId, Is.GreaterThan(spawnedNetId));
        }

        [Test]
        public void SpawnSceneObjectsSendsSpawnedObjectsBeforeUnspawned()
        {
            var spawned = CreateBehaviour<SerializeEventBehaviour>();
            // higher sceneId than unspawned, so that sceneId order alone would put it last
            spawned.Identity.SetSceneId(20);
            serverObjectManager.Spawn(spawned.Identity);

            var unspawned = CreateBehaviour<SerializeEventBehaviour>();
            unspawned.Identity.SetSceneId(10);

            // OnSerialize with initialState is called when the spawn message is created, so it gives the send order
            var sendOrder = new List<NetworkIdentity>();
            spawned.OnSerializeCalled += (initial) =>
            {
                if (initial)
                    sendOrder.Add(spawned.Identity);
            };
            unspawned.OnSerializeCalled += (initial) =>
            {
                if (initial)
                    sendOrder.Add(unspawned.Identity);
            };

            serverObjectManager.SpawnSceneObjects();

            Assert.That(sendOrder.Distinct().ToArray(), Is.EqualTo(new[] { spawned.Identity, unspawned.Identity }));
        }

        [Test]
        public void SpawnSceneObjectsUsesSceneObjectFilterProperty()
        {
            var included = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            included.SetSceneId(10);
            var excluded = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            excluded.SetSceneId(20);

            serverObjectManager.SceneObjectFilter = (identity) => identity == included;
            serverObjectManager.SpawnSceneObjects();

            Assert.That(included.IsSpawned, Is.True);
            Assert.That(excluded.IsSpawned, Is.False);
        }

        [Test]
        public void SpawnSceneObjectsWithFilterOnlySpawnsMatchingObjects()
        {
            var included = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            included.SetSceneId(10);
            var excluded = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            excluded.SetSceneId(20);

            serverObjectManager.SpawnSceneObjects((identity) => identity == included);

            Assert.That(included.IsSpawned, Is.True);
            Assert.That(excluded.IsSpawned, Is.False);
        }

        [Test]
        public void SpawnSceneObjectsWithFilterIgnoresSceneObjectFilterProperty()
        {
            var sceneObject = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            sceneObject.SetSceneId(10);

            // property would exclude everything, so object can only spawn if it is ignored
            serverObjectManager.SceneObjectFilter = (identity) => false;
            serverObjectManager.SpawnSceneObjects((identity) => identity == sceneObject);

            Assert.That(sceneObject.IsSpawned, Is.True);
        }

        [Test]
        public void SpawnSceneObjectsWithNullFilterSpawnsAllSceneObjects()
        {
            var sceneObject = InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>();
            sceneObject.SetSceneId(10);

            // property would exclude everything, so object can only spawn if it is ignored
            serverObjectManager.SceneObjectFilter = (identity) => false;
            serverObjectManager.SpawnSceneObjects(null);

            Assert.That(sceneObject.IsSpawned, Is.True);
        }

        [Test]
        public void SpawnSceneObjectsCalledPerFilterSpawnsGroupsInCallOrder()
        {
            // sceneA has higher sceneIds, so a single unfiltered call would spawn sceneB first
            var sceneA = new[]
            {
                InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>(),
                InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>(),
            };
            sceneA[0].SetSceneId(30);
            sceneA[1].SetSceneId(40);
            var sceneB = new[]
            {
                InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>(),
                InstantiateForTest(_characterPrefabGo).GetComponent<NetworkIdentity>(),
            };
            sceneB[0].SetSceneId(10);
            sceneB[1].SetSceneId(20);

            serverObjectManager.SpawnSceneObjects((identity) => sceneA.Contains(identity));
            serverObjectManager.SpawnSceneObjects((identity) => sceneB.Contains(identity));

            var maxA = sceneA.Max(x => x.NetId);
            var minB = sceneB.Min(x => x.NetId);
            Assert.That(minB, Is.GreaterThan(maxA));
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

