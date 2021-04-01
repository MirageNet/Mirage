using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

namespace Mirage.Tests
{
    public class NetworkWorldTests
    {
        private NetworkWorld world;
        HashSet<uint> existingIds;

        [SetUp]
        public void SetUp()
        {
            world = new NetworkWorld();
            existingIds = new HashSet<uint>();
        }


        void addValidIdentity(out uint id, out NetworkIdentity identity)
        {
            do
            {
                id = (uint)Random.Range(1, 10000);
            }
            while (existingIds.Contains(id));

            existingIds.Add(id);

            identity = new GameObject("WorldTest").AddComponent<NetworkIdentity>();
            identity.NetId = 10;
            world.AddIdentity(id, identity);
        }

        [Test]
        public void StartsEmpty()
        {
            Assert.That(world.SpawnedIdentities.Count, Is.Zero);
        }

        [Test]
        public void TryGetReturnsFalseIfNotFound()
        {
            uint id = 10;
            bool found = world.TryGetIdentity(id, out NetworkIdentity _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsFalseIfNull()
        {
            addValidIdentity(out uint id, out NetworkIdentity identity);

            Object.DestroyImmediate(identity);

            bool found = world.TryGetIdentity(id, out NetworkIdentity _);
            Assert.That(found, Is.False);
        }
        [Test]
        public void TryGetReturnsTrueIfFound()
        {
            addValidIdentity(out uint id, out NetworkIdentity identity);

            bool found = world.TryGetIdentity(id, out NetworkIdentity _);
            Assert.That(found, Is.True);
        }

        [Test, Ignore("not implemneted")] public void AddToCollection() { }
        [Test, Ignore("not implemneted")] public void CanAddManyObjects() { }
        [Test, Ignore("not implemneted")] public void AddInvokesEvent() { }
        [Test, Ignore("not implemneted")] public void AddThrowsIfIdentityIsNull() { }
        [Test, Ignore("not implemneted")] public void AddThrowsIfIdAlreadyInCollection() { }
        [Test, Ignore("not implemneted")] public void AddThrowsIfIdIs0() { }
        [Test, Ignore("not implemneted")] public void AddAssertsIfIdentityDoesNotHaveMatchingId() { }

        [Test, Ignore("not implemneted")] public void RemoveFromCollectionUsingIdentity() { }
        [Test, Ignore("not implemneted")] public void RemoveFromCollectionUsingNetId() { }
        [Test, Ignore("not implemneted")] public void RemoveInvokesEvent() { }
        [Test, Ignore("not implemneted")] public void RemoveThrowsIfIdIs0() { }

        [Test, Ignore("not implemneted")] public void ClearRemovesAllFromCollection() { }
        [Test, Ignore("not implemneted")] public void ClearDoesNotInvokeEvent() { }
    }
}
