using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Mirage.Tests
{
    public class NetworkWorldTests
    {
        private NetworkWorld world;
        private Action<NetworkIdentity> spawnListener;
        private Action<NetworkIdentity> unspawnListener;
        HashSet<uint> existingIds;
        HashSet<byte> existingServerIds;

        [SetUp]
        public void SetUp()
        {
            world = new NetworkWorld();
            spawnListener = Substitute.For<Action<NetworkIdentity>>();
            unspawnListener = Substitute.For<Action<NetworkIdentity>>();
            world.onSpawn += spawnListener;
            world.onUnspawn += unspawnListener;
            existingIds = new HashSet<uint>();
            existingServerIds = new HashSet<byte>();
        }

        void AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity identity)
        {
            id = getValidId();
            serverId = getValidServerId();

            identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
            identity.NetId = id;
            identity.ServerId = serverId;
            world.AddIdentity(id, serverId, identity);
        }

        private uint getValidId()
        {
            uint id;
            do
            {
                id = (uint)Random.Range(1, 10000);
            }
            while (existingIds.Contains(id));

            existingIds.Add(id);
            return id;
        }

        private byte getValidServerId()
        {
            byte id;

            do
            {
                id = (byte)Random.Range(1, byte.MaxValue);

            } while (existingServerIds.Contains(id));

            existingServerIds.Add(id);
            return id;
        }

        [Test]
        public void StartsEmpty()
        {
            Assert.That(world.SpawnedIdentities.Count, Is.Zero);
        }

        [Test]
        public void TryGetReturnsFalseIfNotFound()
        {
            uint id = getValidId();
            byte serverId = getValidServerId();
            bool found = world.TryGetIdentity(id, serverId, out NetworkIdentity _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsFalseIfNull()
        {
            AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity identity);

            Object.DestroyImmediate(identity);

            bool found = world.TryGetIdentity(id, serverId, out NetworkIdentity _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsTrueIfFound()
        {
            AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity identity);

            bool found = world.TryGetIdentity(id, serverId, out NetworkIdentity _);
            Assert.That(found, Is.True);
        }

        [Test]
        public void AddToCollection()
        {
            AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity expected);

            IReadOnlyCollection<NetworkIdentity> collection = world.SpawnedIdentities;

            world.TryGetIdentity(id, serverId, out NetworkIdentity actual);

            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanAddManyObjects()
        {
            AddValidIdentity(out uint id1, out byte serverId1, out NetworkIdentity expected1);
            AddValidIdentity(out uint id2, out byte serverId2, out NetworkIdentity expected2);
            AddValidIdentity(out uint id3, out byte serverId3, out NetworkIdentity expected3);
            AddValidIdentity(out uint id4, out byte serverId4, out NetworkIdentity expected4);

            IReadOnlyCollection<NetworkIdentity> collection = world.SpawnedIdentities;

            world.TryGetIdentity(id1, serverId1, out NetworkIdentity actual1);
            world.TryGetIdentity(id2, serverId2, out NetworkIdentity actual2);
            world.TryGetIdentity(id3, serverId3, out NetworkIdentity actual3);
            world.TryGetIdentity(id4, serverId4, out NetworkIdentity actual4);

            Assert.That(collection.Count, Is.EqualTo(4));
            Assert.That(actual1, Is.EqualTo(expected1));
            Assert.That(actual2, Is.EqualTo(expected2));
            Assert.That(actual3, Is.EqualTo(expected3));
            Assert.That(actual4, Is.EqualTo(expected4));
        }
        [Test]
        public void AddInvokesEvent()
        {
            AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity expected);

            spawnListener.Received(1).Invoke(expected);
        }
        [Test]
        public void AddInvokesEventOncePerAdd()
        {
            AddValidIdentity(out uint id1, out byte serverId1, out NetworkIdentity expected1);
            AddValidIdentity(out uint id2, out byte serverId2, out NetworkIdentity expected2);
            AddValidIdentity(out uint id3, out byte serverId3, out NetworkIdentity expected3);
            AddValidIdentity(out uint id4, out byte serverId4, out NetworkIdentity expected4);

            spawnListener.Received(1).Invoke(expected1);
            spawnListener.Received(1).Invoke(expected2);
            spawnListener.Received(1).Invoke(expected3);
            spawnListener.Received(1).Invoke(expected4);
        }
        [Test]
        public void AddThrowsIfIdentityIsNull()
        {
            uint id = getValidId();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            {
                world.AddIdentity(id, 1, null);
            });

            var expected = new ArgumentNullException("identity");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }
        [Test]
        public void AddThrowsIfIdAlreadyInCollection()
        {
            AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity identity);

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id, serverId, identity);
            });

            var expected = new ArgumentException($"NetId:{id} ServerId:{serverId} resulted in an id already exists in dictionary.", "uniqueId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void AddThrowsIfNetIdIs0()
        {
            uint id = 0;
            byte serverId = 1;
            NetworkIdentity identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
            identity.NetId = id;
            identity.ServerId = serverId;

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id, serverId, identity);
            });

            var expected = new ArgumentException("net id can not be zero", "netId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }
        [Test]
        public void AddThrowsIfServerIdIs0()
        {
            uint id = 1;
            byte serverId = 0;
            NetworkIdentity identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
            identity.NetId = id;
            identity.ServerId = serverId;

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id, serverId, identity);
            });

            var expected = new ArgumentException("server id can not be zero", "serverId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }
        [Test]
        public void AddAssertsIfIdentityDoesNotHaveMatchingId()
        {
            uint id1 = getValidId();
            uint id2 = getValidId();

            NetworkIdentity identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
            identity.NetId = id1;

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id2, 1, identity);
            });

            var expected = new ArgumentException("NetworkIdentity did not have matching netId", "identity");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void RemoveFromCollectionUsingIdentity()
        {
            AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity identity);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(1));

            world.RemoveIdentity(identity);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(0));

            Assert.That(world.TryGetIdentity(id, serverId, out NetworkIdentity identityOut), Is.False);
            Assert.That(identityOut, Is.EqualTo(null));
        }
        [Test]
        public void RemoveFromCollectionUsingNetId()
        {
            AddValidIdentity(out uint id, out byte serverId, out NetworkIdentity identity);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(1));

            world.RemoveIdentity(id, serverId);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(0));

            Assert.That(world.TryGetIdentity(id, serverId, out NetworkIdentity identityOut), Is.False);
            Assert.That(identityOut, Is.EqualTo(null));
        }
        [Test]
        public void RemoveOnlyRemovesCorrectItem()
        {
            AddValidIdentity(out uint id1, out byte serverId1, out NetworkIdentity identity1);
            AddValidIdentity(out uint id2, out byte serverId2, out NetworkIdentity identity2);
            AddValidIdentity(out uint id3, out byte serverId3, out NetworkIdentity identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.RemoveIdentity(identity2);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(2));

            Assert.That(world.TryGetIdentity(id1, serverId1, out NetworkIdentity identityOut1), Is.True);
            Assert.That(world.TryGetIdentity(id2, serverId2, out NetworkIdentity identityOut2), Is.False);
            Assert.That(world.TryGetIdentity(id3, serverId3, out NetworkIdentity identityOut3), Is.True);
            Assert.That(identityOut1, Is.EqualTo(identity1));
            Assert.That(identityOut2, Is.EqualTo(null));
            Assert.That(identityOut3, Is.EqualTo(identity3));
        }
        [Test]
        public void RemoveInvokesEvent()
        {
            AddValidIdentity(out uint id1, out byte serverId1, out NetworkIdentity identity1);
            AddValidIdentity(out uint id2, out byte serverId2, out NetworkIdentity identity2);
            AddValidIdentity(out uint id3, out byte serverId3, out NetworkIdentity identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.RemoveIdentity(identity3);
            unspawnListener.Received(1).Invoke(identity3);

            world.RemoveIdentity(id1, serverId1);
            unspawnListener.Received(1).Invoke(identity1);

            world.RemoveIdentity(id2, serverId2);
            unspawnListener.Received(1).Invoke(identity2);
        }

        [Test]
        public void RemoveThrowsIfNetIdIs0()
        {
            uint id = 0;
            byte serverId = 1;

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.RemoveIdentity(id, serverId);
            });

            var expected = new ArgumentException("net id can not be zero", "netId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            unspawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void RemoveThrowsIfServerIdIs0()
        {
            uint id = 1;
            byte serverId = 0;

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.RemoveIdentity(id, serverId);
            });

            var expected = new ArgumentException("server id can not be zero", "serverId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            unspawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void ClearRemovesAllFromCollection()
        {
            AddValidIdentity(out uint id1, out byte serverId1, out NetworkIdentity identity1);
            AddValidIdentity(out uint id2, out byte serverId2, out NetworkIdentity identity2);
            AddValidIdentity(out uint id3, out byte serverId3, out NetworkIdentity identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.ClearSpawnedObjects();
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(0));
        }
        [Test]
        public void ClearDoesNotInvokeEvent()
        {
            AddValidIdentity(out uint id1, out byte serverId1, out NetworkIdentity identity1);
            AddValidIdentity(out uint id2, out byte serverId2, out NetworkIdentity identity2);
            AddValidIdentity(out uint id3, out byte serverId3, out NetworkIdentity identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.ClearSpawnedObjects();
            unspawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void ClearDestoryedObjects()
        {
            AddValidIdentity(out uint id1, out byte serverId1, out NetworkIdentity identity1);
            AddValidIdentity(out uint id2, out byte serverId2, out NetworkIdentity identity2);
            AddValidIdentity(out uint id3, out byte serverId3, out NetworkIdentity identity3);
            AddValidIdentity(out uint id4, out byte serverId4, out NetworkIdentity nullIdentity);

            Object.DestroyImmediate(nullIdentity);

            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(4));

            world.RemoveDestroyedObjects();

            foreach (NetworkIdentity identity in world.SpawnedIdentities)
            {
                Assert.That(identity != null);
            }

            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));
        }

        [UnityTest, Explicit]
        public IEnumerator TestNetworkWorldDictionaryCollisions() => UniTask.ToCoroutine(async () =>
        {
            NetworkIdentity identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();

            for (uint x = 1; x < uint.MaxValue; x++)
            {
                for (byte y = 1; y < byte.MaxValue; y++)
                {
                    identity.NetId = x;
                    identity.ServerId = y;
                    world.AddIdentity(x, y, identity);
                }

                while (EditorApplication.isUpdating)
                {
                    await UniTask.Delay(5);
                }
            }
        });
    }
}
