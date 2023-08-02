using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class MockPlayerWithList : MockPlayer
    {
        public SyncList<int> MySyncList = new SyncList<int>();
    }

    // different Directions to test

    // server -> owner
    // server -> observer
    // server -> owner,observer

    // owner -> server
    // owner -> server,observer

    // owner,server -> owner,server
    // owner,server -> server,observer
    // owner,server -> owner,server,observer

    public class SyncDirectionObjectFromServer : SyncDirectionTestBase<MockPlayerWithList>
    {
        private const int listValue = 5;

        [UnityTest]
        public IEnumerator ToOwner()
        {
            SetDirection(SyncFrom.Server, SyncTo.Owner);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;
            ServerComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ToObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.ObserversOnly);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;
            ServerComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(OwnerComponent.target, Is.Null);
            Assert.That(OwnerComponent.MySyncList.Count, Is.Zero);

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue));
        }

        [UnityTest]
        public IEnumerator ToOwnerAndObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.OwnerAndObservers);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;
            ServerComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue));
        }

        [UnityTest]
        public IEnumerator CanAddInStartServer()
        {
            var clone = InstantiateForTest(_characterPrefab);
            var component = clone.GetComponent<MockPlayerWithList>();
            clone.OnStartServer.AddListener(() =>
            {
                component.MySyncList.Add(listValue);
            });

            serverObjectManager.Spawn(clone);

            // wait for sync
            yield return null;
            yield return null;

            var clientObj = _remoteClients[0].Get(component);

            Assert.That(clientObj.MySyncList.Count, Is.EqualTo(1));
            Assert.That(clientObj.MySyncList[0], Is.EqualTo(listValue));
        }
    }

    public class SyncDirectionObjectFromServer_AddCharacter : SyncDirectionTestBase<MockPlayerWithList>
    {
        private const int listValue = 5;

        protected override bool SpawnCharacterOnConnect => false;

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            base.ExtraPrefabSetup(prefab);

            SetDirection(prefab.GetComponent<MockPlayerWithList>(), SyncFrom.Server, SyncTo.Owner | SyncTo.ObserversOnly);
        }

        [UnityTest]
        public IEnumerator CanAddInStartServer()
        {
            var components = new MockPlayerWithList[RemoteClientCount];
            for (var i = 0; i < RemoteClientCount; i++)
            {
                var clone = InstantiateForTest(_characterPrefab);
                components[i] = clone.GetComponent<MockPlayerWithList>();
                clone.OnStartServer.AddListener(() =>
                {
                    components[i].MySyncList.Add(listValue);
                });

                serverObjectManager.AddCharacter(ServerPlayer(i), clone);
            }

            // wait for sync
            yield return null;
            yield return null;

            for (var i = 0; i < RemoteClientCount; i++)
            {
                // check all objects on all clients
                for (var j = 0; j < RemoteClientCount; j++)
                {
                    var clientObj = _remoteClients[i].Get(components[j]);

                    Assert.That(clientObj.MySyncList.Count, Is.EqualTo(1));
                    Assert.That(clientObj.MySyncList[0], Is.EqualTo(listValue));
                }
            }
        }

        [UnityTest]
        public IEnumerator CanSetValuesBeforeSpawn()
        {
            var clone = InstantiateForTest(_characterPrefab);
            var component = clone.GetComponent<MockPlayerWithList>();

            Assert.DoesNotThrow(() =>
            {
                component.MySyncList.Add(listValue);
            }, "Server should be able too set syncList value when object is despawned");
            serverObjectManager.AddCharacter(ServerPlayer(0), clone);

            // wait for sync
            yield return null;
            yield return null;

            var clientObj = _remoteClients[0].Get(component);

            Assert.That(clientObj.MySyncList.Count, Is.EqualTo(1));
            Assert.That(clientObj.MySyncList[0], Is.EqualTo(listValue));
        }

        [UnityTest]
        public IEnumerator CanSetValuesAfterDestroy()
        {
            var clone = InstantiateForTest(_characterPrefab);
            var component = clone.GetComponent<MockPlayerWithList>();
            serverObjectManager.AddCharacter(ServerPlayer(0), clone);

            // wait for spawn
            yield return null;
            yield return null;

            serverObjectManager.Destroy(component.Identity, false);

            // wait for despawn
            yield return null;
            yield return null;

            Assert.DoesNotThrow(() =>
            {
                component.MySyncList.Add(listValue);
            }, "Server should be able too set syncList value when object is despawned");
            serverObjectManager.AddCharacter(ServerPlayer(0), component.Identity);

            // wait for spawn
            yield return null;
            yield return null;

            // dont use OwnerComponent, it will be null because of destroy
            var clientObj = _remoteClients[0].Get(component);

            Assert.That(clientObj.MySyncList.Count, Is.EqualTo(1));
            Assert.That(clientObj.MySyncList[0], Is.EqualTo(listValue));
        }
    }

    public class SyncDirectionObjectFromOwner : SyncDirectionTestBase<MockPlayerWithList>
    {
        private const int listValue = 5;

        [UnityTest]
        public IEnumerator ToServer()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            OwnerComponent.guild = guild;
            OwnerComponent.target = _remoteClients[0].Get(ServerExtraIdentity);
            OwnerComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            OwnerComponent.guild = guild;
            OwnerComponent.target = _remoteClients[0].Get(ServerExtraIdentity);
            OwnerComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue));
        }

        [Test]
        public void ThrowsWhenServerUpdates()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);
            ServerComponent.UpdateSyncObjectShouldSync();

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                ServerComponent.MySyncList.Add(listValue);
            });

            var expected = new InvalidOperationException("SyncObject is marked as ReadOnly. Check SyncDirection and make sure you can set values on this instance. By default you can only add items on server.");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
    }

    public class SyncDirectionObjectFromOwner_Spawning : SyncDirectionTestBase<MockPlayerWithList>
    {
        private const int listValue = 5;

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            base.ExtraPrefabSetup(prefab);

            SetDirection(prefab.GetComponent<MockPlayerWithList>(), SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);
        }

        [UnityTest]
        public IEnumerator CanRespawn()
        {
            OwnerComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            serverObjectManager.Destroy(ServerComponent.Identity, false);

            // wait for despawn
            yield return null;
            yield return null;

            serverObjectManager.Spawn(ServerComponent.Identity);

            // wait for spawn
            yield return null;
            yield return null;

            // dont use OwnerComponent, it will be null because of destroy
            var clientObj = _remoteClients[0].Get(ServerComponent);

            // NOTE: should be empty, because destroy will reset the list
            Assert.That(clientObj.MySyncList.Count, Is.EqualTo(0));
        }

        [UnityTest]
        [Ignore("todo this doesn't sync initial values. What should happen?")]
        public IEnumerator CanSetValuesAfterDestroy()
        {
            serverObjectManager.Destroy(ServerComponent.Identity, false);

            // wait for despawn
            yield return null;
            yield return null;

            Assert.DoesNotThrow(() =>
            {
                ServerComponent.MySyncList.Add(listValue);
            }, "Server should be able too set syncList value when object is despawned");
            serverObjectManager.Spawn(ServerComponent.Identity);

            // wait for spawn
            yield return null;
            yield return null;

            // dont use OwnerComponent, it will be null because of destroy
            var clientObj = _remoteClients[0].Get(ServerComponent);

            Assert.That(clientObj.MySyncList.Count, Is.EqualTo(1));
            Assert.That(clientObj.MySyncList[0], Is.EqualTo(listValue));
        }

        [UnityTest]
        [Ignore("todo this doesn't sync initial values. What should happen?")]
        public IEnumerator CanSetValuesBeforeSpawn()
        {
            var clone = InstantiateForTest(_characterPrefab);
            var component = clone.GetComponent<MockPlayerWithList>();

            Assert.DoesNotThrow(() =>
            {
                component.MySyncList.Add(listValue);
            }, "Server should be able too set syncList value when object is despawned");
            serverObjectManager.Spawn(clone);

            // wait for sync
            yield return null;
            yield return null;

            var clientObj = _remoteClients[0].Get(component);

            Assert.That(clientObj.MySyncList.Count, Is.EqualTo(1));
            Assert.That(clientObj.MySyncList[0], Is.EqualTo(listValue));
        }

        [UnityTest]
        [Ignore("todo this doesn't sync initial values. What should happen?")]
        public IEnumerator CanSpawnOnSecondClient()
        {
            SetDirection(_characterPrefab.GetComponent<MockPlayerWithList>(), SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            var clone = InstantiateForTest(_characterPrefab);
            var component = clone.GetComponent<MockPlayerWithList>();

            serverObjectManager.ReplaceCharacter(ServerPlayer(0), clone, false);

            // wait for spawn
            yield return null;
            yield return null;

            var newOwnerComp = _remoteClients[0].Get(component);
            newOwnerComp.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(component.MySyncList.Count, Is.EqualTo(1));
            Assert.That(component.MySyncList[0], Is.EqualTo(listValue));

            var newIndex = _remoteClients.Count;
            var addTask = AddClient();
            yield return new WaitWhile(() => addTask.Status == UniTaskStatus.Pending);

            var newClientComp = _remoteClients[newIndex].Get(component);
            Assert.That(newClientComp.MySyncList.Count, Is.EqualTo(1));
            Assert.That(newClientComp.MySyncList[0], Is.EqualTo(listValue));
        }
    }

    public class SyncDirectionObjectFromServerAndOwner : SyncDirectionTestBase<MockPlayerWithList>
    {
        private const int listValue1 = 5;
        private const int listValue2 = 10;

        [UnityTest]
        public IEnumerator ToServerAndOwner()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;
            ServerComponent.MySyncList.Add(listValue1);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue1));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);

            OwnerComponent.guild = guild2;
            OwnerComponent.target = null;
            OwnerComponent.MySyncList.Add(listValue2);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(OwnerComponent.MySyncList[1], Is.EqualTo(listValue2));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ServerComponent.target, Is.Null);
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(ServerComponent.MySyncList[1], Is.EqualTo(listValue2));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;
            ServerComponent.MySyncList.Add(listValue1);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(OwnerComponent.target, Is.Null);
            Assert.That(OwnerComponent.MySyncList.Count, Is.Zero);

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue1));

            // just update guild
            OwnerComponent.guild = guild2;
            OwnerComponent.MySyncList.Add(listValue2);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue2));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild2.name));
            // target should not be changed
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(ServerComponent.MySyncList[1], Is.EqualTo(listValue2));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(ObserverComponent.MySyncList[1], Is.EqualTo(listValue2));
        }

        [UnityTest]
        public IEnumerator ToServerOwnerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;
            ServerComponent.MySyncList.Add(listValue1);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue1));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue1));

            OwnerComponent.guild = guild2;
            OwnerComponent.target = null;
            OwnerComponent.MySyncList.Add(listValue2);


            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(OwnerComponent.MySyncList[1], Is.EqualTo(listValue2));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ServerComponent.target, Is.Null);
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(ServerComponent.MySyncList[1], Is.EqualTo(listValue2));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ObserverComponent.target, Is.Null);
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(ObserverComponent.MySyncList[1], Is.EqualTo(listValue2));

            OwnerComponent.MySyncList.Clear();

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);
            Assert.That(OwnerComponent.MySyncList.Count, Is.Zero);

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ServerComponent.target, Is.Null);
            Assert.That(ServerComponent.MySyncList.Count, Is.Zero);

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ObserverComponent.target, Is.Null);
            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator CanSetDifferentVarsOnDifferentFrom()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            OwnerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

            ServerComponent.MySyncList.Add(listValue1);
            OwnerComponent.MySyncList.Add(listValue2);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(2));
            // owner added 2 first, then received 1 from server
            // this could lead to desync, but that is a risk of using by sync directions 
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue2));
            Assert.That(OwnerComponent.MySyncList[1], Is.EqualTo(listValue1));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));
            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(ServerComponent.MySyncList[1], Is.EqualTo(listValue2));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator CanSetDifferentVarsOnDifferentFrom_onlySyncObject()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            ServerComponent.MySyncList.Add(listValue1);
            OwnerComponent.MySyncList.Add(listValue2);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.MySyncList.Count, Is.EqualTo(2));
            // owner added 2 first, then received 1 from server
            // this could lead to desync, but that is a risk of using by sync directions
            Assert.That(OwnerComponent.MySyncList[0], Is.EqualTo(listValue2));
            Assert.That(OwnerComponent.MySyncList[1], Is.EqualTo(listValue1));

            Assert.That(ServerComponent.MySyncList.Count, Is.EqualTo(2));
            Assert.That(ServerComponent.MySyncList[0], Is.EqualTo(listValue1));
            Assert.That(ServerComponent.MySyncList[1], Is.EqualTo(listValue2));

            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);
        }
    }
}
