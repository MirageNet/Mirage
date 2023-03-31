using System;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class NetworkIdentityTests : ClientServerSetup<MockComponent>
    {
        [Test]
        public void IsServer()
        {
            Assert.That(serverIdentity.IsServer, Is.True);
            Assert.That(clientIdentity.IsServer, Is.False);
        }

        [Test]
        public void IsClient()
        {
            Assert.That(serverIdentity.IsClient, Is.False);
            Assert.That(clientIdentity.IsClient, Is.True);
        }

        [Test]
        public void IsLocalPlayer()
        {
            Assert.That(serverIdentity.IsLocalPlayer, Is.False);
            Assert.That(clientIdentity.IsLocalPlayer, Is.True);
        }

        [Test]
        public void DefaultAuthority()
        {
            Assert.That(serverIdentity.Owner, Is.EqualTo(serverPlayer));
            Assert.That(clientIdentity.Owner, Is.Null);
        }

        [Test]
        public void ThrowsIfAssignAuthorityCalledOnClient()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                clientIdentity.AssignClientAuthority(clientPlayer);
            });
        }

        [Test]
        public void ThrowsIfRemoteAuthorityCalledOnClient()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // shoud fail because the server is not active
                clientIdentity.RemoveClientAuthority();
            });
        }

        [Test]
        public void RemoveAuthority()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // shoud fail because the server is not active
                clientIdentity.RemoveClientAuthority();
            });
        }

        [Test]
        public void IsSceneObject()
        {
            var clone = CreateNetworkIdentity();

            clone.SetSceneId(40);
            Assert.That(clone.IsSceneObject, Is.True);
        }
        [Test]
        public void IsNotSceneObject()
        {
            var clone = CreateNetworkIdentity();

            clone.SetSceneId(0);
            Assert.That(clone.IsSceneObject, Is.False);
        }
        [Test]
        public void IsPrefab()
        {
            var clone = CreateNetworkIdentity();

            clone.PrefabHash = 23232;
            Assert.That(clone.IsPrefab, Is.True);
        }
        [Test]
        public void IsNotPrefab()
        {
            var clone = CreateNetworkIdentity();

            clone.Editor_PrefabHash = 0;
            Assert.That(clone.IsPrefab, Is.False);
        }
        [Test]
        public void IsNotPrefabIfScenObject()
        {
            var clone = CreateNetworkIdentity();

            clone.PrefabHash = 23232;
            clone.SetSceneId(422);
            Assert.That(clone.IsPrefab, Is.False);
        }
        [Test]
        public void IsSpawned()
        {
            var clone = CreateNetworkIdentity();
            clone.NetId = 20;

            Assert.That(clone.IsSpawned, Is.True);
        }
        [Test]
        public void IsNotSpawned()
        {
            var clone = CreateNetworkIdentity();
            clone.NetId = 0;

            Assert.That(clone.IsSpawned, Is.False);
        }
    }
}
