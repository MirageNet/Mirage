using System;
using Mirage.Serialization;
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
            var now = Time.unscaledTimeAsDouble;
            var player = CreateBehaviour<MockPlayer>();
            player._nextSyncTime = now + 1f;
            player.SyncSettings.Interval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.ShouldSync(now), Is.False, "Sync interval not met, so not dirty yet");

            // ClearDirtyComponents should do nothing since syncInterval is not
            // elapsed yet
            player.Identity.ClearShouldSyncDirtyOnly();

            // set lastSyncTime far enough back to be ready for syncing
            player._nextSyncTime = now - 1f;

            // should be dirty now
            Assert.That(player.ShouldSync(now), Is.True, "Sync interval met, should be dirty");
        }

        [Test]
        public void TestSyncIntervalAndClearAllComponents()
        {
            var now = Time.unscaledTimeAsDouble;
            var player = CreateBehaviour<MockPlayer>();
            player._nextSyncTime = now + 1f;
            player.SyncSettings.Interval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.ShouldSync(now), Is.False, "Sync interval not met, so not dirty yet");

            // ClearAllComponents should clear dirty even if syncInterval not
            // elapsed yet
            player.Identity.ClearShouldSync(now);

            // set lastSyncTime far enough back to be ready for syncing
            player._nextSyncTime = now - 1f;

            // should be dirty now
            Assert.That(player.ShouldSync(now), Is.False, "Sync interval met, should still not be dirty");
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
            var (ownerWritten, observersWritten) = player1.Identity.OnSerializeInitial(ownerWriter, observersWriter);

            Assert.That(ownerWritten, Is.EqualTo(0), "no owner, should have only written to observersWriter");
            Assert.That(observersWritten, Is.GreaterThanOrEqualTo(1), "should have written to observer writer");

            // set up a "client" object
            var player2 = CreateBehaviour<MockPlayer>();

            // apply all the data from the server object
            reader.Reset(observersWriter.ToArray());
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
