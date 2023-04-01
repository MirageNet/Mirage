using System;
using Mirage.Serialization;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Syncing
{
    public class MockPlayer : NetworkBehaviour
    {
        public struct Guild
        {
            public string name;

            public Guild(string name)
            {
                this.name = name;
            }
        }

        [SyncVar]
        public Guild guild;

        [SyncVar]
        public NetworkIdentity target;


        public event Action OnSerializeCalled;
        public event Action OnDeserializeCalled;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            OnSerializeCalled?.Invoke();
            return base.OnSerialize(writer, initialState);
        }
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            OnDeserializeCalled?.Invoke();
            base.OnDeserialize(reader, initialState);
        }
    }

    public class SyncVarTest : ClientServerSetup<MockPlayer>
    {
        private readonly NetworkWriter ownerWriter = new NetworkWriter(1300);
        private readonly NetworkWriter observersWriter = new NetworkWriter(1300);
        private readonly MirageNetworkReader reader = new MirageNetworkReader();

        [TearDown]
        public void TearDown()
        {
            ownerWriter.Reset();
            observersWriter.Reset();
            reader.Dispose();
        }


        [Test]
        public void TestSettingStruct()
        {
            var player = CreateBehaviour<MockPlayer>();

            Assert.That(player.AnyDirtyBits(), Is.False, "First time object should not be dirty");

            var myGuild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            player.guild = myGuild;

            Assert.That(player.AnyDirtyBits(), "Setting struct should mark object as dirty");
            player.ClearDirtyBits();
            Assert.That(player.AnyDirtyBits(), Is.False, "ClearAllDirtyBits() should clear dirty flag");

            // clearing the guild should set dirty bit too
            player.guild = default;
            Assert.That(player.AnyDirtyBits(), "Clearing struct should mark object as dirty");
        }

        [Test]
        public void TestSyncIntervalAndClearDirtyComponents()
        {
            var player = CreateBehaviour<MockPlayer>();
            player._nextSyncTime = Time.time + 1f;
            player.SyncSettings.Interval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.ShouldSync(Time.time), Is.False, "Sync interval not met, so not dirty yet");

            // ClearDirtyComponents should do nothing since syncInterval is not
            // elapsed yet
            player.Identity.ClearShouldSyncDirtyOnly();

            // set lastSyncTime far enough back to be ready for syncing
            player._nextSyncTime = Time.time - 1f;

            // should be dirty now
            Assert.That(player.ShouldSync(Time.time), Is.True, "Sync interval met, should be dirty");
        }

        [Test]
        public void TestSyncIntervalAndClearAllComponents()
        {
            var player = CreateBehaviour<MockPlayer>();
            player._nextSyncTime = Time.time + 1f;
            player.SyncSettings.Interval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.ShouldSync(Time.time), Is.False, "Sync interval not met, so not dirty yet");

            // ClearAllComponents should clear dirty even if syncInterval not
            // elapsed yet
            player.Identity.ClearShouldSync();

            // set lastSyncTime far enough back to be ready for syncing
            player._nextSyncTime = Time.time - 1f;

            // should be dirty now
            Assert.That(player.ShouldSync(Time.time), Is.False, "Sync interval met, should still not be dirty");
        }

        [Test]
        public void TestSynchronizingObjects()
        {
            // set up a "server" object
            var player1 = CreateBehaviour<MockPlayer>();
            var myGuild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };
            player1.guild = myGuild;

            // spawn so server value is set
            serverObjectManager.Spawn(player1.Identity);

            //serialize all the data as we would for the network
            player1.Identity.OnSerializeAll(true, ownerWriter, observersWriter);

            // set up a "client" object
            var player2 = CreateBehaviour<MockPlayer>();

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
            var player = CreateBehaviour<MockPlayer>();
            var other = CreateNetworkIdentity();

            player.target = other.GetComponent<NetworkIdentity>();

            Assert.That(player.target, Is.EqualTo(other.GetComponent<NetworkIdentity>()));
        }
    }
}
