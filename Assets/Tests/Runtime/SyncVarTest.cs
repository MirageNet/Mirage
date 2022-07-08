using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Serialization
{
    class MockPlayer : NetworkBehaviour
    {
        public struct Guild
        {
            public string name;
        }

        [SyncVar]
        public Guild guild;

        [SyncVar]
        public NetworkIdentity target;
    }

    public class SyncVarTest : TestBase
    {
        readonly NetworkWriter ownerWriter = new NetworkWriter(1300);
        readonly NetworkWriter observersWriter = new NetworkWriter(1300);
        readonly MirageNetworkReader reader = new MirageNetworkReader();

        [TearDown]
        public void TearDown()
        {
            ownerWriter.Reset();
            observersWriter.Reset();
            reader.Dispose();

            TearDownTestObjects();
        }


        [Test]
        public void TestSettingStruct()
        {
            MockPlayer player = CreateBehaviour<MockPlayer>();

            // synchronize immediatelly
            player.syncInterval = 0f;

            Assert.That(player.IsDirty(), Is.False, "First time object should not be dirty");

            var myGuild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            player.guild = myGuild;

            Assert.That(player.IsDirty(), "Setting struct should mark object as dirty");
            player.ClearAllDirtyBits();
            Assert.That(player.IsDirty(), Is.False, "ClearAllDirtyBits() should clear dirty flag");

            // clearing the guild should set dirty bit too
            player.guild = default;
            Assert.That(player.IsDirty(), "Clearing struct should mark object as dirty");
        }

        [Test]
        public void TestSyncIntervalAndClearDirtyComponents()
        {
            MockPlayer player = CreateBehaviour<MockPlayer>();
            player.lastSyncTime = Time.time;
            // synchronize immediately
            player.syncInterval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.IsDirty(), Is.False, "Sync interval not met, so not dirty yet");

            // ClearDirtyComponents should do nothing since syncInterval is not
            // elapsed yet
            player.Identity.ClearDirtyComponentsDirtyBits();

            // set lastSyncTime far enough back to be ready for syncing
            player.lastSyncTime = Time.time - player.syncInterval;

            // should be dirty now
            Assert.That(player.IsDirty(), Is.True, "Sync interval met, should be dirty");
        }

        [Test]
        public void TestSyncIntervalAndClearAllComponents()
        {
            MockPlayer player = CreateBehaviour<MockPlayer>();
            player.lastSyncTime = Time.time;
            // synchronize immediately
            player.syncInterval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.IsDirty(), Is.False, "Sync interval not met, so not dirty yet");

            // ClearAllComponents should clear dirty even if syncInterval not
            // elapsed yet
            player.Identity.ClearAllComponentsDirtyBits();

            // set lastSyncTime far enough back to be ready for syncing
            player.lastSyncTime = Time.time - player.syncInterval;

            // should be dirty now
            Assert.That(player.IsDirty(), Is.False, "Sync interval met, should still not be dirty");
        }

        [Test]
        public void TestSynchronizingObjects()
        {
            // set up a "server" object
            MockPlayer player1 = CreateBehaviour<MockPlayer>();
            var myGuild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };
            player1.guild = myGuild;

            // serialize all the data as we would for the network
            player1.Identity.OnSerializeAll(true, ownerWriter, observersWriter);

            // set up a "client" object
            MockPlayer player2 = CreateBehaviour<MockPlayer>();

            // apply all the data from the server object
            reader.Reset(ownerWriter.ToArray());
            player2.Identity.OnDeserializeAll(reader, true);

            // check that the syncvars got updated
            Assert.That(player2.guild.name, Is.EqualTo("Back street boys"), "Data should be synchronized");
        }

        [Test]
        [Description("Syncvars are converted to properties behind the scenes, this tests makes sure you can set and get them")]
        public void CanSetAndGetNetworkIdentitySyncvar()
        {
            MockPlayer player = CreateBehaviour<MockPlayer>();
            NetworkIdentity other = CreateNetworkIdentity();

            player.target = other.GetComponent<NetworkIdentity>();

            Assert.That(player.target, Is.EqualTo(other.GetComponent<NetworkIdentity>()));
        }
    }
}
