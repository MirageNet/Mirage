using Mirage.RemoteCalls;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests
{
    public class NetworkIdentityAuthorityGracePeriodTests : TestBase
    {
        private NetworkIdentity identity;
        private INetworkPlayer player1;
        private INetworkPlayer player2;

        [SetUp]
        public void SetUp()
        {
            identity = CreateNetworkIdentity();
            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }

        [Test]
        public void FalseIfAuthorityNeverRemoved()
        {
            Assert.That(identity.InAuthorityRemovedGracePeriod(player1), Is.False);
        }

        [Test]
        public void TrueAfterRemovingOwner()
        {
            // use SetOwner so the test also covers AuthorityRemovedTime being set
            identity.SetOwner(player1);
            identity.SetOwner(null);

            Assert.That(identity.InAuthorityRemovedGracePeriod(player1), Is.True);
        }

        [Test]
        public void FalseForOtherPlayer()
        {
            identity.SetOwner(player1);
            identity.SetOwner(null);

            Assert.That(identity.InAuthorityRemovedGracePeriod(player2), Is.False);
        }

        [Test]
        public void FalseForCurrentOwnerThatNeverLostAuthority()
        {
            identity.SetOwner(player1);

            Assert.That(identity.InAuthorityRemovedGracePeriod(player1), Is.False);
        }

        [Test]
        public void TrueJustBeforeGracePeriodEnds()
        {
            // set time directly so the test doesn't have to wait for the grace period
            var removedTime = Time.unscaledTimeAsDouble - RpcHandler.DESTROY_GRACE_PERIOD + 1;
            identity.AuthorityRemovedTime = new AuthorityTime(player1, removedTime);

            Assert.That(identity.InAuthorityRemovedGracePeriod(player1), Is.True);
        }

        [Test]
        public void FalseAfterGracePeriodEnds()
        {
            var removedTime = Time.unscaledTimeAsDouble - RpcHandler.DESTROY_GRACE_PERIOD - 1;
            identity.AuthorityRemovedTime = new AuthorityTime(player1, removedTime);

            Assert.That(identity.InAuthorityRemovedGracePeriod(player1), Is.False);
        }

        [Test]
        public void OnlyMostRecentPreviousOwnerIsInGracePeriod()
        {
            identity.SetOwner(player1);
            identity.SetOwner(null);
            identity.SetOwner(player2);
            identity.SetOwner(null);

            Assert.That(identity.InAuthorityRemovedGracePeriod(player1), Is.False);
            Assert.That(identity.InAuthorityRemovedGracePeriod(player2), Is.True);
        }
    }
}
