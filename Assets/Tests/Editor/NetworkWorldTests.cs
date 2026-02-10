using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Mirage.Tests
{
    public class NetworkWorldTests : TestBase
    {
        private NetworkWorld world;
        private Action<NetworkIdentity> spawnListener;
        private NetworkWorld.UnspawnHandler unspawnListener;
        private HashSet<uint> existingIds;

        [SetUp]
        public void SetUp()
        {
            world = new NetworkWorld();
            spawnListener = Substitute.For<Action<NetworkIdentity>>();
            unspawnListener = Substitute.For<NetworkWorld.UnspawnHandler>();
            world.onSpawn += spawnListener;
            world.onUnspawn += unspawnListener;
            existingIds = new HashSet<uint>();
        }
        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }

        private void AddValidIdentity(out uint id, out NetworkIdentity identity)
        {
            id = getValidId();

            identity = CreateNetworkIdentity();
            identity.NetId = id;
            world.AddIdentity(id, identity);
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

        [Test]
        public void StartsEmpty()
        {
            Assert.That(world.SpawnedIdentities.Count, Is.Zero);
        }

        [Test]
        public void TryGetReturnsFalseIfNotFound()
        {
            var id = getValidId();
            var found = world.TryGetIdentity(id, out var _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsFalseIfNull()
        {
            AddValidIdentity(out var id, out var identity);

            Object.DestroyImmediate(identity);

            var found = world.TryGetIdentity(id, out var _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsTrueIfFound()
        {
            AddValidIdentity(out var id, out var identity);

            var found = world.TryGetIdentity(id, out var _);
            Assert.That(found, Is.True);
        }

        [Test]
        public void AddToCollection()
        {
            AddValidIdentity(out var id, out var expected);

            var collection = world.SpawnedIdentities;

            world.TryGetIdentity(id, out var actual);

            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanAddManyObjects()
        {
            AddValidIdentity(out var id1, out var expected1);
            AddValidIdentity(out var id2, out var expected2);
            AddValidIdentity(out var id3, out var expected3);
            AddValidIdentity(out var id4, out var expected4);

            var collection = world.SpawnedIdentities;

            world.TryGetIdentity(id1, out var actual1);
            world.TryGetIdentity(id2, out var actual2);
            world.TryGetIdentity(id3, out var actual3);
            world.TryGetIdentity(id4, out var actual4);

            Assert.That(collection.Count, Is.EqualTo(4));
            Assert.That(actual1, Is.EqualTo(expected1));
            Assert.That(actual2, Is.EqualTo(expected2));
            Assert.That(actual3, Is.EqualTo(expected3));
            Assert.That(actual4, Is.EqualTo(expected4));
        }
        [Test]
        public void AddInvokesEvent()
        {
            AddValidIdentity(out var id, out var expected);

            spawnListener.Received(1).Invoke(expected);
        }
        [Test]
        public void AddInvokesEventOncePerAdd()
        {
            AddValidIdentity(out var id1, out var expected1);
            AddValidIdentity(out var id2, out var expected2);
            AddValidIdentity(out var id3, out var expected3);
            AddValidIdentity(out var id4, out var expected4);

            spawnListener.Received(1).Invoke(expected1);
            spawnListener.Received(1).Invoke(expected2);
            spawnListener.Received(1).Invoke(expected3);
            spawnListener.Received(1).Invoke(expected4);
        }
        [Test]
        public void AddThrowsIfIdentityIsNull()
        {
            var id = getValidId();

            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                world.AddIdentity(id, null);
            });

            var expected = new ArgumentNullException("identity");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }
        [Test]
        public void AddThrowsIfIdAlreadyInCollection()
        {
            AddValidIdentity(out var id, out var identity);

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id, identity);
            });

            var expected = new ArgumentException("An Identity with same id already exists in network world", "netId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void AddThrowsIfIdIs0()
        {
            uint id = 0;
            var identity = CreateNetworkIdentity();
            identity.NetId = id;

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id, identity);
            });

            var expected = new ArgumentException("id can not be zero", "netId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }
        [Test]
        public void AddAssertsIfIdentityDoesNotHaveMatchingId()
        {
            var id1 = getValidId();
            var id2 = getValidId();

            var identity = CreateNetworkIdentity();
            identity.NetId = id1;


            var exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id2, identity);
            });

            var expected = new ArgumentException("NetworkIdentity did not have matching netId", "identity");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void RemoveFromCollectionUsingIdentity()
        {
            AddValidIdentity(out var id, out var identity);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(1));

            world.RemoveIdentity(identity);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(0));

            Assert.That(world.TryGetIdentity(id, out var identityOut), Is.False);
            Assert.That(identityOut, Is.EqualTo(null));
        }
        [Test]
        public void RemoveFromCollectionUsingNetId()
        {
            AddValidIdentity(out var id, out var identity);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(1));

            world.RemoveIdentity(id);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(0));

            Assert.That(world.TryGetIdentity(id, out var identityOut), Is.False);
            Assert.That(identityOut, Is.EqualTo(null));
        }
        [Test]
        public void RemoveOnlyRemovesCorrectItem()
        {
            AddValidIdentity(out var id1, out var identity1);
            AddValidIdentity(out var id2, out var identity2);
            AddValidIdentity(out var id3, out var identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.RemoveIdentity(identity2);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(2));

            Assert.That(world.TryGetIdentity(id1, out var identityOut1), Is.True);
            Assert.That(world.TryGetIdentity(id2, out var identityOut2), Is.False);
            Assert.That(world.TryGetIdentity(id3, out var identityOut3), Is.True);
            Assert.That(identityOut1, Is.EqualTo(identity1));
            Assert.That(identityOut2, Is.EqualTo(null));
            Assert.That(identityOut3, Is.EqualTo(identity3));
        }
        [Test]
        public void RemoveInvokesEvent()
        {
            AddValidIdentity(out var id1, out var identity1);
            AddValidIdentity(out var id2, out var identity2);
            AddValidIdentity(out var id3, out var identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.RemoveIdentity(identity3);
            unspawnListener.Received(1).Invoke(id3, identity3);

            world.RemoveIdentity(id1);
            unspawnListener.Received(1).Invoke(id1, identity1);

            world.RemoveIdentity(id2);
            unspawnListener.Received(1).Invoke(id2, identity2);
        }

        [Test]
        public void RemoveThrowsIfIdIs0()
        {
            uint id = 0;

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                world.RemoveIdentity(id);
            });

            var expected = new ArgumentException("id can not be zero", "netId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            unspawnListener.DidNotReceiveWithAnyArgs().Invoke(default, default);
        }

        [Test]
        public void ClearRemovesAllFromCollection()
        {
            AddValidIdentity(out var id1, out var identity1);
            AddValidIdentity(out var id2, out var identity2);
            AddValidIdentity(out var id3, out var identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.ClearSpawnedObjects();
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(0));
        }
        [Test]
        public void ClearDoesNotInvokeEvent()
        {
            AddValidIdentity(out var id1, out var identity1);
            AddValidIdentity(out var id2, out var identity2);
            AddValidIdentity(out var id3, out var identity3);
            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));

            world.ClearSpawnedObjects();
            unspawnListener.DidNotReceiveWithAnyArgs().Invoke(default, default);
        }

        [Test]
        public void ClearDestoryedObjects()
        {
            AddValidIdentity(out var id1, out var identity1);
            AddValidIdentity(out var id2, out var identity2);
            AddValidIdentity(out var id3, out var identity3);
            AddValidIdentity(out var id4, out var nullIdentity);

            Object.DestroyImmediate(nullIdentity);

            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(4));

            world.RemoveDestroyedObjects();

            foreach (var identity in world.SpawnedIdentities)
            {
                Assert.That(identity != null);
            }

            Assert.That(world.SpawnedIdentities.Count, Is.EqualTo(3));
        }
    }
}
