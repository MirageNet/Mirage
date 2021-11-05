using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
            NetworkIdentity clone = CreateNetworkIdentity();

            clone.SetSceneId(40);
            Assert.That(clone.IsSceneObject, Is.True);
        }
        [Test]
        public void IsNotSceneObject()
        {
            NetworkIdentity clone = CreateNetworkIdentity();

            clone.SetSceneId(0);
            Assert.That(clone.IsSceneObject, Is.False);
        }
        [Test]
        public void IsPrefab()
        {
            NetworkIdentity clone = CreateNetworkIdentity();

            clone.PrefabHash = 23232;
            Assert.That(clone.IsPrefab, Is.True);
        }
        [Test]
        public void IsNotPrefab()
        {
            NetworkIdentity clone = CreateNetworkIdentity();

            clone.PrefabHash = 0;
            Assert.That(clone.IsPrefab, Is.False);
        }
        [Test]
        public void IsNotPrefabIfScenObject()
        {
            NetworkIdentity clone = CreateNetworkIdentity();

            clone.PrefabHash = 23232;
            clone.SetSceneId(422);
            Assert.That(clone.IsPrefab, Is.False);
        }
        [Test]
        public void IsSpawned()
        {
            NetworkIdentity clone = CreateNetworkIdentity();
            clone.NetId = 20;

            Assert.That(clone.IsSpawned, Is.True);
        }
        [Test]
        public void IsNotSpawned()
        {
            NetworkIdentity clone = CreateNetworkIdentity();
            clone.NetId = 0;

            Assert.That(clone.IsSpawned, Is.False);
        }
    }

    public class NetworkIdentityAuthorityTests : ClientServerSetup<MockComponent>
    {
        NetworkIdentity serverIdentity2;
        NetworkIdentity clientIdentity2;

        public override async UniTask LateSetup()
        {
            base.ExtraSetup();

            serverIdentity2 = GameObject.Instantiate(playerPrefab).GetComponent<NetworkIdentity>();
            serverObjectManager.Spawn(serverIdentity2);

            await UniTask.DelayFrame(2);

            client.World.TryGetIdentity(serverIdentity2.NetId, out clientIdentity2);
            Debug.Assert(clientIdentity2 != null);
        }

        public override void ExtraTearDown()
        {
            base.ExtraTearDown();

            if (serverIdentity2 != null)
                GameObject.Destroy(serverIdentity2);
            if (clientIdentity2 != null)
                GameObject.Destroy(clientIdentity2);
        }

        [UnityTest]
        public IEnumerator AssignAuthority()
        {
            serverIdentity2.AssignClientAuthority(serverPlayer);
            Assert.That(serverIdentity2.Owner, Is.EqualTo(serverPlayer));

            yield return new WaitForSeconds(0.1f);
            Assert.That(clientIdentity2.HasAuthority, Is.True);
        }

        [UnityTest]
        public IEnumerator RemoveClientAuthority()
        {
            serverIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            serverIdentity2.RemoveClientAuthority();
            Assert.That(serverIdentity2.Owner, Is.EqualTo(null));

            yield return new WaitForSeconds(0.1f);
            Assert.That(clientIdentity2.HasAuthority, Is.False);
        }

        [UnityTest]
        public IEnumerator RemoveClientAuthority_DoesNotResetPosition()
        {
            serverIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            // set position on client
            var clientPosition = new Vector3(200, -30, 40);
            clientIdentity2.transform.position = clientPosition;
            serverIdentity2.transform.position = Vector3.zero;

            // remove auth on server
            serverIdentity2.RemoveClientAuthority();

            yield return new WaitForSeconds(0.1f);
            // expect authority to be gone, but position not to be reset
            Debug.Assert(clientIdentity2.HasAuthority == false);
            Assert.That(clientIdentity2.transform.position, Is.EqualTo(clientPosition));
            Assert.That(serverIdentity2.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        [Description("OnAuthorityChanged should not be called on server side")]
        public void OnAuthorityChanged_Server()
        {
            var hasAuthCalls = new Queue<bool>();
            serverIdentity2.OnAuthorityChanged.AddListener(hasAuth =>
            {
                hasAuthCalls.Enqueue(hasAuth);
            });

            serverIdentity2.AssignClientAuthority(serverPlayer);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(0));

            serverIdentity2.RemoveClientAuthority();

            Assert.That(hasAuthCalls.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator OnAuthorityChanged_Client()
        {
            var hasAuthCalls = new Queue<bool>();
            clientIdentity2.OnAuthorityChanged.AddListener(hasAuth =>
            {
                hasAuthCalls.Enqueue(hasAuth);
            });

            serverIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.True);

            serverIdentity2.RemoveClientAuthority();
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.False);
        }
    }
}
