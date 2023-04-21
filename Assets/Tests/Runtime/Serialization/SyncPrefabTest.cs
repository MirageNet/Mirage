using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class SyncPrefabTest : TestBase
    {
        private readonly NetworkWriter writer = new NetworkWriter(1300);
        private readonly MirageNetworkReader reader = new MirageNetworkReader();

        [TearDown]
        public void TearDown()
        {
            reader.Dispose();
            writer.Reset();
            TearDownTestObjects();
        }

        [Test]
        public void SendsPrefabHash()
        {
            const int hash = 300;
            var inValue = new SyncPrefab(hash);

            writer.Write(inValue);
            reader.Reset(writer.ToArraySegment());
            var outValue = reader.Read<SyncPrefab>();

            Assert.That(outValue.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void SendsPrefabHashFromIdentity()
        {
            var identity = CreateNetworkIdentity();
            const int hash = 320;
            identity.PrefabHash = hash;

            var inValue = new SyncPrefab(identity);

            writer.Write(inValue);
            reader.Reset(writer.ToArraySegment());
            var outValue = reader.Read<SyncPrefab>();

            Assert.That(outValue.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void SendsZeroIfIdentityNull()
        {
            var inValue = new SyncPrefab(null);

            writer.Write(inValue);
            reader.Reset(writer.ToArraySegment());
            var outValue = reader.Read<SyncPrefab>();

            Assert.That(outValue.PrefabHash, Is.EqualTo(0));
        }

        [Test]
        public void GetsHashFromPrefabEvenIfFieldIsSet()
        {
            var identity = CreateNetworkIdentity();
            const int hash = 330;
            identity.PrefabHash = hash;
            var inValue = new SyncPrefab()
            {
                Prefab = identity,
                PrefabHash = 400 // not the same hash
            };

            writer.Write(inValue);
            reader.Reset(writer.ToArraySegment());
            var outValue = reader.Read<SyncPrefab>();

            Assert.That(outValue.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void GetsHashFromPrefabEvenIfFieldIsSetUnlessPrefabHashIsZeroo()
        {
            var identity = CreateNetworkIdentity();
            const int hash = 350;
            var inValue = new SyncPrefab()
            {
                Prefab = identity,
                PrefabHash = hash // not the same hash
            };

            writer.Write(inValue);
            reader.Reset(writer.ToArraySegment());
            var outValue = reader.Read<SyncPrefab>();

            Assert.That(outValue.PrefabHash, Is.EqualTo(hash));
        }


        [Test]
        public void FindsPrefabInList()
        {
            const int hash = 410;
            var sync = new SyncPrefab(hash);


            var identity1 = CreateNetworkIdentity();
            identity1.PrefabHash = 400;
            var identity2 = CreateNetworkIdentity();
            identity2.PrefabHash = 410;
            var identity3 = CreateNetworkIdentity();
            identity3.PrefabHash = 420;
            var identity4 = CreateNetworkIdentity();
            identity4.PrefabHash = 430;

            var list = new List<NetworkIdentity>() { identity1, identity2, identity3, identity4 };

            var found = sync.FindPrefab(list);
            Assert.That(found, Is.EqualTo(identity2));
        }

        [Test]
        public void DoesNotSearchIfIdentityFieldAlreadySet()
        {
            const int hash = 410;
            var sync = new SyncPrefab(hash);


            var identity1 = CreateNetworkIdentity();
            identity1.PrefabHash = 400;
            var identity2 = CreateNetworkIdentity();
            identity2.PrefabHash = 410;
            var identity3 = CreateNetworkIdentity();
            identity3.PrefabHash = 420;
            var identity4 = CreateNetworkIdentity();
            identity4.PrefabHash = 430;

            var list = new List<NetworkIdentity>() { identity1, identity2, identity3, identity4 };

            sync.Prefab = identity3;

            var found = sync.FindPrefab(list);
            Assert.That(found, Is.EqualTo(identity3), "should return field instead of looking through list");
        }
    }

    public class SyncPrefabTestWithCOM : ClientServerSetup<MockComponent>
    {
        private readonly NetworkWriter writer = new NetworkWriter(1300);
        private readonly MirageNetworkReader reader = new MirageNetworkReader();

        [TearDown]
        public void TearDown()
        {
            reader.Dispose();
            writer.Reset();
            TearDownTestObjects();
        }

        [Test]
        public void FindsPrefabInClientObjectManager()
        {
            const int hash = 410;
            var sync = new SyncPrefab(hash);

            var identity1 = CreateNetworkIdentity();
            identity1.PrefabHash = 400;
            var identity2 = CreateNetworkIdentity();
            identity2.PrefabHash = 410;
            var identity3 = CreateNetworkIdentity();
            identity3.PrefabHash = 420;
            var identity4 = CreateNetworkIdentity();
            identity4.PrefabHash = 430;

            clientObjectManager.RegisterPrefab(identity1);
            clientObjectManager.RegisterPrefab(identity2);
            clientObjectManager.RegisterPrefab(identity3);
            clientObjectManager.RegisterPrefab(identity4);

            var found = sync.FindPrefab(clientObjectManager);
            Assert.That(found, Is.EqualTo(identity2));
        }

        [Test]
        public void DoesNotSearchIfIdentityFieldAlreadySet()
        {
            const int hash = 410;
            var sync = new SyncPrefab(hash);

            var identity1 = CreateNetworkIdentity();
            identity1.PrefabHash = 400;
            var identity2 = CreateNetworkIdentity();
            identity2.PrefabHash = 410;
            var identity3 = CreateNetworkIdentity();
            identity3.PrefabHash = 420;
            var identity4 = CreateNetworkIdentity();
            identity4.PrefabHash = 430;

            clientObjectManager.RegisterPrefab(identity1);
            clientObjectManager.RegisterPrefab(identity2);
            clientObjectManager.RegisterPrefab(identity3);
            clientObjectManager.RegisterPrefab(identity4);

            sync.Prefab = identity3;

            var found = sync.FindPrefab(clientObjectManager);
            Assert.That(found, Is.EqualTo(identity3), "should return field instead of looking through list");
        }
    }
}
