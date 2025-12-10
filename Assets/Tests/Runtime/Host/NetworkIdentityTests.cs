using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using InvalidOperationException = System.InvalidOperationException;

namespace Mirage.Tests.Runtime.Host
{
    public class NetworkIdentityTests : HostSetup
    {
        private GameObject gameObject;
        private NetworkIdentity testIdentity;

        protected override async UniTask ExtraSetup()
        {
            await base.ExtraSetup();
            testIdentity = CreateNetworkIdentity();
            gameObject = testIdentity.gameObject;
        }

        [Test]
        public void AssignClientAuthorityNoServer()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                testIdentity.AssignClientAuthority(server.LocalPlayer);
            });
        }

        [Test]
        public void IsServer()
        {
            Assert.That(testIdentity.IsServer, Is.False);
            // create a networkidentity with our test component
            serverObjectManager.Spawn(gameObject);

            Assert.That(testIdentity.IsServer, Is.True);
        }

        [Test]
        public void IsClient()
        {
            Assert.That(testIdentity.IsClient, Is.False);
            // create a networkidentity with our test component
            serverObjectManager.Spawn(gameObject);

            Assert.That(testIdentity.IsClient, Is.True);
        }

        [Test]
        public void IsLocalPlayer()
        {
            Assert.That(testIdentity.IsLocalPlayer, Is.False);
            // create a networkidentity with our test component
            serverObjectManager.Spawn(gameObject);

            Assert.That(testIdentity.IsLocalPlayer, Is.False);
        }

        [Test]
        public void DefaultAuthority()
        {
            // create a networkidentity with our test component
            serverObjectManager.Spawn(gameObject);
            Assert.That(testIdentity.Owner, Is.Null);
        }

        [Test]
        public void AssignAuthority()
        {
            // create a networkidentity with our test component
            serverObjectManager.Spawn(gameObject);
            testIdentity.AssignClientAuthority(server.LocalPlayer);

            Assert.That(testIdentity.Owner, Is.SameAs(server.LocalPlayer));
        }

        [Test]
        public void SpawnWithAuthority()
        {
            serverObjectManager.Spawn(gameObject, server.LocalPlayer);
            Assert.That(testIdentity.Owner, Is.SameAs(server.LocalPlayer));
        }

        [Test]
        public void SpawnGameObjectWithPrefabHash()
        {
            var hash = Guid.NewGuid().GetHashCode();
            serverObjectManager.Spawn(gameObject, hash, server.LocalPlayer);
            Assert.That(testIdentity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void SpawnNetworkIdentityWithPrefabHash()
        {
            var hash = Guid.NewGuid().GetHashCode();
            serverObjectManager.Spawn(testIdentity, hash, server.LocalPlayer);
            Assert.That(testIdentity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void ReassignClientAuthority()
        {
            // create a networkidentity with our test component
            serverObjectManager.Spawn(gameObject);
            // assign authority
            testIdentity.AssignClientAuthority(server.LocalPlayer);

            // shouldn't be able to assign authority while already owned by
            // another connection
            Assert.Throws<InvalidOperationException>(() =>
            {
                testIdentity.AssignClientAuthority(Substitute.For<INetworkPlayer>());
            });
        }

        [Test]
        public void AssignNullAuthority()
        {
            // create a networkidentity with our test component
            serverObjectManager.Spawn(gameObject);

            // someone might try to remove authority by assigning null.
            // make sure this fails.
            Assert.Throws<ArgumentNullException>(() =>
            {
                testIdentity.AssignClientAuthority(null);
            });
        }

        [Test]
        public void RemoveclientAuthorityNotSpawned()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // shoud fail because the server is not active
                testIdentity.RemoveClientAuthority();
            });
        }

        [Test]
        public void RemoveClientAuthorityOfOwner()
        {
            serverObjectManager.ReplaceCharacter(server.LocalPlayer, gameObject);

            Assert.Throws<InvalidOperationException>(() =>
            {
                testIdentity.RemoveClientAuthority();
            });
        }

        [Test]
        public void RemoveClientAuthority()
        {
            serverObjectManager.Spawn(gameObject);
            testIdentity.AssignClientAuthority(server.LocalPlayer);
            testIdentity.RemoveClientAuthority();
            Assert.That(testIdentity.Owner, Is.Null);
            Assert.That(testIdentity.HasAuthority, Is.False);
            Assert.That(testIdentity.IsLocalPlayer, Is.False);
        }

        [UnityTest]
        public IEnumerator OnStopServer() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.Spawn(gameObject);

            var mockHandler = Substitute.For<Action>();
            testIdentity.OnStopServer.AddListener(mockHandler);

            serverObjectManager.Destroy(gameObject, false);

            await UniTask.Delay(1);
            mockHandler.Received().Invoke();
        });

        [Test]
        public void IdentityClientValueSet()
        {
            Assert.That(hostIdentity.Client, Is.Not.Null);
        }

        [Test]
        public void IdentityServerValueSet()
        {
            Assert.That(hostIdentity.Server, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DestroyOwnedObjectsTest() => UniTask.ToCoroutine(async () =>
        {
            var testObj1 = CreateNetworkIdentity();
            var testObj2 = CreateNetworkIdentity();
            var testObj3 = CreateNetworkIdentity();

            // only destroys spawned objects, so spawn them here
            serverObjectManager.Spawn(testObj1);
            serverObjectManager.Spawn(testObj2);
            serverObjectManager.Spawn(testObj3);

            server.LocalPlayer.AddOwnedObject(testObj1);
            server.LocalPlayer.AddOwnedObject(testObj2);
            server.LocalPlayer.AddOwnedObject(testObj3);
            server.LocalPlayer.DestroyOwnedObjects();

            await AsyncUtil.WaitUntilWithTimeout(() => !testObj1);
            await AsyncUtil.WaitUntilWithTimeout(() => !testObj2);
            await AsyncUtil.WaitUntilWithTimeout(() => !testObj3);
        });
    }

    public class NetworkIdentityStartedTests : HostSetup
    {
        private NetworkIdentity testIdentity;

        protected override UniTask LateSetup()
        {
            testIdentity = CreateNetworkIdentity();
            server.Started.AddListener(() => serverObjectManager.Spawn(testIdentity));

            return UniTask.CompletedTask;
        }

        [UnityTest]
        public IEnumerator ClientNotNullAfterSpawnInStarted() => UniTask.ToCoroutine(async () =>
        {
            await AsyncUtil.WaitUntilWithTimeout(() => testIdentity.Client == client);
        });
    }
}
