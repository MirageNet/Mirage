using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using InvalidOperationException = System.InvalidOperationException;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.Host
{
    public class NetworkIdentityTests : HostSetup<MockComponent>
    {
        #region SetUp

        GameObject gameObject;
        NetworkIdentity testIdentity;

        public override void ExtraSetup()
        {
            gameObject = new GameObject();
            testIdentity = gameObject.AddComponent<NetworkIdentity>();
        }

        public override void ExtraTearDown()
        {
            Object.Destroy(gameObject);
        }

        #endregion

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
        public void SpawnWithPrefabHash()
        {
            int hash = Guid.NewGuid().GetHashCode();
            serverObjectManager.Spawn(gameObject, hash, server.LocalPlayer);
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
            Assert.Throws<InvalidOperationException>(() =>
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

            UnityAction mockHandler = Substitute.For<UnityAction>();
            testIdentity.OnStopServer.AddListener(mockHandler);

            serverObjectManager.Destroy(gameObject, false);

            await UniTask.Delay(1);
            mockHandler.Received().Invoke();
        });

        [Test]
        public void IdentityClientValueSet()
        {
            Assert.That(identity.Client, Is.Not.Null);
        }

        [Test]
        public void IdentityServerValueSet()
        {
            Assert.That(identity.Server, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DestroyOwnedObjectsTest() => UniTask.ToCoroutine(async () =>
        {
            NetworkIdentity testObj1 = new GameObject().AddComponent<NetworkIdentity>();
            NetworkIdentity testObj2 = new GameObject().AddComponent<NetworkIdentity>();
            NetworkIdentity testObj3 = new GameObject().AddComponent<NetworkIdentity>();

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

    public class NetworkIdentityStartedTests : HostSetup<MockComponent>
    {
        #region SetUp

        GameObject gameObject;
        NetworkIdentity testIdentity;

        public override void ExtraSetup()
        {
            gameObject = new GameObject();
            testIdentity = gameObject.AddComponent<NetworkIdentity>();
            server.Started.AddListener(() => serverObjectManager.Spawn(gameObject));
        }

        public override void ExtraTearDown()
        {
            Object.Destroy(gameObject);
        }

        #endregion

        [UnityTest]
        public IEnumerator ClientNotNullAfterSpawnInStarted() => UniTask.ToCoroutine(async () =>
        {
            await AsyncUtil.WaitUntilWithTimeout(() => (testIdentity.Client as NetworkClient) == client);
        });
    }
}
