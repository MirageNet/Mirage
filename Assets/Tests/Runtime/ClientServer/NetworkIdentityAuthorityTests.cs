using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class NetworkIdentityAuthorityTests : ClientServerSetup<MockComponent>
    {
        private NetworkIdentity serverIdentity2;
        private NetworkIdentity clientIdentity2;
        private NetworkWorld ClientWorld => client.World;
        private NetworkWorld ServerWorld => server.World;

        public override async UniTask LateSetup()
        {
            base.ExtraSetup();


            serverIdentity2 = InstantiateForTest(playerPrefab).GetComponent<NetworkIdentity>();
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

        [Test]
        public void OnOwnerChanged_Server()
        {
            var hasAuthCalls = new Queue<INetworkPlayer>();
            serverIdentity2.OnOwnerChanged.AddListener(newOwner =>
            {
                hasAuthCalls.Enqueue(newOwner);
            });

            serverIdentity2.AssignClientAuthority(serverPlayer);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.EqualTo(serverPlayer));

            serverIdentity2.RemoveClientAuthority();

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.Null);
        }

        [UnityTest]
        [Description("OnOwnerChanged should not be called on client side")]
        public IEnumerator OnOwnerChanged_Client()
        {
            var hasAuthCalls = new Queue<INetworkPlayer>();
            clientIdentity2.OnOwnerChanged.AddListener(newOwner =>
            {
                hasAuthCalls.Enqueue(newOwner);
            });

            serverIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(0));

            serverIdentity2.RemoveClientAuthority();
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(0));
        }

        [Test]
        public void WorldOnAuthorityChanged_Server()
        {
            var hasAuthCalls = new Queue<(NetworkIdentity identity, bool hasAuthority, INetworkPlayer owner)>();
            ServerWorld.OnAuthorityChanged += (identity, hasAuth, owner) =>
            {
                hasAuthCalls.Enqueue((identity, hasAuth, owner));
            };

            serverIdentity2.AssignClientAuthority(serverPlayer);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            var first = hasAuthCalls.Dequeue();
            Assert.That(first.identity, Is.EqualTo(serverIdentity2));
            Assert.That(first.hasAuthority, Is.EqualTo(true));
            Assert.That(first.owner, Is.EqualTo(serverPlayer));

            serverIdentity2.RemoveClientAuthority();

            // 1 again, because we dequeued first event
            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            var second = hasAuthCalls.Dequeue();
            Assert.That(second.identity, Is.EqualTo(serverIdentity2));
            Assert.That(second.hasAuthority, Is.EqualTo(false));
            Assert.That(second.owner, Is.EqualTo(serverPlayer));
        }

        [UnityTest]
        public IEnumerator WorldOnAuthorityChanged_Client()
        {
            var hasAuthCalls = new Queue<(NetworkIdentity identity, bool hasAuthority, INetworkPlayer owner)>();
            ClientWorld.OnAuthorityChanged += (identity, hasAuth, owner) =>
            {
                hasAuthCalls.Enqueue((identity, hasAuth, owner));
            };

            serverIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            var first = hasAuthCalls.Dequeue();
            Assert.That(first.identity, Is.EqualTo(clientIdentity2));
            Assert.That(first.hasAuthority, Is.EqualTo(true));
            Assert.That(first.owner, Is.Null.Or.EqualTo(clientPlayer));

            serverIdentity2.RemoveClientAuthority();
            yield return new WaitForSeconds(0.1f);

            // 1 again, because we dequeued first event
            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            var second = hasAuthCalls.Dequeue();
            Assert.That(second.identity, Is.EqualTo(clientIdentity2));
            Assert.That(second.hasAuthority, Is.EqualTo(false));
            Assert.That(second.owner, Is.Null.Or.EqualTo(clientPlayer));
        }
    }
}
