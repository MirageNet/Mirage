using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class HostNetworkIdentityAuthorityTests : HostSetup<MockComponent>
    {
        private NetworkIdentity hostIdentity2;
        private NetworkWorld HostWorld => server.World;
        private INetworkPlayer serverPlayer => server.LocalPlayer;

        protected override UniTask LateSetup()
        {
            hostIdentity2 = CreateBehaviour<MockComponent>().GetComponent<NetworkIdentity>();
            serverObjectManager.Spawn(hostIdentity2);

            return UniTask.CompletedTask;
        }

        [UnityTest]
        public IEnumerator AssignAuthority()
        {
            hostIdentity2.AssignClientAuthority(serverPlayer);
            Assert.That(hostIdentity2.Owner, Is.EqualTo(serverPlayer));

            yield return new WaitForSeconds(0.1f);
            Assert.That(hostIdentity2.HasAuthority, Is.True);
        }

        [UnityTest]
        public IEnumerator RemoveClientAuthority()
        {
            hostIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            hostIdentity2.RemoveClientAuthority();
            Assert.That(hostIdentity2.Owner, Is.EqualTo(null));

            yield return new WaitForSeconds(0.1f);
            Assert.That(hostIdentity2.HasAuthority, Is.False);
        }

        [UnityTest]
        [Description("OnAuthorityChanged should be invoked once on host")]
        public IEnumerator OnAuthorityChanged_Client()
        {
            var hasAuthCalls = new Queue<bool>();
            hostIdentity2.OnAuthorityChanged.AddListener(hasAuth =>
            {
                hasAuthCalls.Enqueue(hasAuth);
            });

            hostIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.True);

            hostIdentity2.RemoveClientAuthority();
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.False);
        }


        [UnityTest]
        [Description("OnOwnerChanged should be invoked once on host")]
        public IEnumerator OnOwnerChanged_Client()
        {
            var hasAuthCalls = new Queue<INetworkPlayer>();
            hostIdentity2.OnOwnerChanged.AddListener(newOwner =>
            {
                hasAuthCalls.Enqueue(newOwner);
            });

            hostIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.EqualTo(serverPlayer));

            hostIdentity2.RemoveClientAuthority();
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            Assert.That(hasAuthCalls.Dequeue(), Is.Null);
        }

        [UnityTest]
        [Description("OnOwnerChanged should be invoked once on host")]
        public IEnumerator WorldOnAuthorityChanged()
        {
            var hasAuthCalls = new Queue<(NetworkIdentity identity, bool hasAuthority, INetworkPlayer owner)>();
            HostWorld.OnAuthorityChanged += (identity, hasAuth, owner) =>
            {
                hasAuthCalls.Enqueue((identity, hasAuth, owner));
            };

            hostIdentity2.AssignClientAuthority(serverPlayer);
            yield return new WaitForSeconds(0.1f);

            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            var first = hasAuthCalls.Dequeue();
            Assert.That(first.identity, Is.EqualTo(hostIdentity2));
            Assert.That(first.hasAuthority, Is.EqualTo(true));
            Assert.That(first.owner, Is.EqualTo(serverPlayer));

            hostIdentity2.RemoveClientAuthority();
            yield return new WaitForSeconds(0.1f);

            // 1 again, because we dequeued first event
            Assert.That(hasAuthCalls.Count, Is.EqualTo(1));
            var second = hasAuthCalls.Dequeue();
            Assert.That(second.identity, Is.EqualTo(hostIdentity2));
            Assert.That(second.hasAuthority, Is.EqualTo(false));
            Assert.That(second.owner, Is.EqualTo(serverPlayer));
        }
    }
}
