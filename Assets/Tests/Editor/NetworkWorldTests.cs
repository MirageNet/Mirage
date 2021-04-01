using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
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

        [SetUp]
        public void SetUp()
        {
            world = new NetworkWorld();
            spawnListener = Substitute.For<Action<NetworkIdentity>>();
            unspawnListener = Substitute.For<Action<NetworkIdentity>>();
            world.onSpawn += spawnListener;
            world.onUnspawn += unspawnListener;
            existingIds = new HashSet<uint>();
        }

        void AddValidIdentity(out uint id, out NetworkIdentity identity)
        {
            id = getValidId();

            identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
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
            uint id = getValidId();
            bool found = world.TryGetIdentity(id, out NetworkIdentity _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsFalseIfNull()
        {
            AddValidIdentity(out uint id, out NetworkIdentity identity);

            Object.DestroyImmediate(identity);

            bool found = world.TryGetIdentity(id, out NetworkIdentity _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsTrueIfFound()
        {
            AddValidIdentity(out uint id, out NetworkIdentity identity);

            bool found = world.TryGetIdentity(id, out NetworkIdentity _);
            Assert.That(found, Is.True);
        }

        [Test]
        public void AddToCollection()
        {
            AddValidIdentity(out uint id, out NetworkIdentity expected);

            IReadOnlyCollection<NetworkIdentity> collection = world.SpawnedIdentities;

            world.TryGetIdentity(id, out NetworkIdentity actual);

            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanAddManyObjects()
        {
            AddValidIdentity(out uint id1, out NetworkIdentity expected1);
            AddValidIdentity(out uint id2, out NetworkIdentity expected2);
            AddValidIdentity(out uint id3, out NetworkIdentity expected3);
            AddValidIdentity(out uint id4, out NetworkIdentity expected4);

            IReadOnlyCollection<NetworkIdentity> collection = world.SpawnedIdentities;

            world.TryGetIdentity(id1, out NetworkIdentity actual1);
            world.TryGetIdentity(id2, out NetworkIdentity actual2);
            world.TryGetIdentity(id3, out NetworkIdentity actual3);
            world.TryGetIdentity(id4, out NetworkIdentity actual4);

            Assert.That(collection.Count, Is.EqualTo(4));
            Assert.That(actual1, Is.EqualTo(expected1));
            Assert.That(actual2, Is.EqualTo(expected2));
            Assert.That(actual3, Is.EqualTo(expected3));
            Assert.That(actual4, Is.EqualTo(expected4));
        }
        [Test]
        public void AddInvokesEvent()
        {
            AddValidIdentity(out uint id, out NetworkIdentity expected);

            spawnListener.Received(1).Invoke(expected);
        }
        [Test]
        public void AddInvokesEventOncePerAdd()
        {
            AddValidIdentity(out uint id1, out NetworkIdentity expected1);
            AddValidIdentity(out uint id2, out NetworkIdentity expected2);
            AddValidIdentity(out uint id3, out NetworkIdentity expected3);
            AddValidIdentity(out uint id4, out NetworkIdentity expected4);

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
                world.AddIdentity(id, null);
            });

            var expected = new ArgumentNullException("identity");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }
        [Test]
        public void AddThrowsIfIdAlreadyInCollection()
        {
            AddValidIdentity(out uint id, out NetworkIdentity identity);

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id, identity);
            });

            var expected = new ArgumentException("An item with same id already exists", "netId");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void AddThrowsIfIdIs0()
        {
            uint id = 0;
            NetworkIdentity identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
            identity.NetId = id;

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
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
            uint id1 = getValidId();
            uint id2 = getValidId();

            NetworkIdentity identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
            identity.NetId = id1;


            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                world.AddIdentity(id2, identity);
            });

            var expected = new ArgumentException("NetworkIdentity did not have matching netId", "identity");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));

            spawnListener.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test] public void RemoveFromCollectionUsingIdentity() { Assert.Ignore("NotImplemented"); }
        [Test] public void RemoveFromCollectionUsingNetId() { Assert.Ignore("NotImplemented"); }
        [Test] public void RemoveInvokesEvent() { Assert.Ignore("NotImplemented"); }
        [Test] public void RemoveThrowsIfIdIs0() { Assert.Ignore("NotImplemented"); }

        [Test] public void ClearRemovesAllFromCollection() { Assert.Ignore("NotImplemented"); }
        [Test] public void ClearDoesNotInvokeEvent() { Assert.Ignore("NotImplemented"); }
    }
}
