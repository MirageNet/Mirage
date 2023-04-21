using Mirage.Collections;
using NUnit.Framework;

using static Mirage.Tests.LocalConnections;

namespace Mirage.Tests.Runtime.Host
{
    public class SampleBehavior : NetworkBehaviour
    {
    }

    public class NetworkBehaviourTests : HostSetup<SampleBehavior>
    {
        [Test]
        public void IsServerOnly()
        {
            Assert.That(hostComponent.IsServerOnly, Is.False);
        }

        [Test]
        public void IsServer()
        {
            Assert.That(hostComponent.IsServer, Is.True);
        }

        [Test]
        public void IsClient()
        {
            Assert.That(hostComponent.IsClient, Is.True);
        }

        [Test]
        public void IsClientOnly()
        {
            Assert.That(hostComponent.IsClientOnly, Is.False);
        }

        [Test]
        public void PlayerHasAuthorityByDefault()
        {
            // no authority by default
            Assert.That(hostComponent.HasAuthority, Is.True);
        }

        private class OnStartServerTestComponent : NetworkBehaviour
        {
            public bool called;

            public void OnStartServer()
            {
                Assert.That(IsClient, Is.True);
                Assert.That(IsLocalPlayer, Is.False);
                Assert.That(IsServer, Is.True);
                called = true;
            }
        };

        // check isClient/isServer/isLocalPlayer in server-only mode
        [Test]
        public void OnStartServer()
        {
            var gameObject = CreateGameObject();
            var netIdentity = gameObject.AddComponent<NetworkIdentity>();
            var comp = gameObject.AddComponent<OnStartServerTestComponent>();
            netIdentity.OnStartServer.AddListener(comp.OnStartServer);

            Assert.That(comp.called, Is.False);
            serverObjectManager.Spawn(gameObject);

            Assert.That(comp.called, Is.True);
        }

        [Test]
        public void SpawnedObjectNoAuthority()
        {
            var behaviour = CreateBehaviour<SampleBehavior>();
            serverObjectManager.Spawn(behaviour.gameObject);

            client.Update();

            // no authority by default
            Assert.That(behaviour.HasAuthority, Is.False);
        }

        [Test]
        public void HasIdentitysNetId()
        {
            hostIdentity.NetId = 42;
            Assert.That(hostComponent.NetId, Is.EqualTo(42));
        }

        [Test]
        public void ReturnsCorrectBehaviourId()
        {
            hostIdentity.NetId = 42;

            var compIndex = hostComponent.ComponentIndex;

            var id = hostComponent.BehaviourId;
            Assert.That(id.NetId, Is.EqualTo(42));
            Assert.That(id.ComponentIndex, Is.EqualTo(compIndex));
        }

        [Test]
        public void NetworkBehaviourIdEquals()
        {
            var id1 = new NetworkBehaviour.Id(10, 2);
            var id2 = new NetworkBehaviour.Id(10, 2);

            Assert.IsTrue(id1.Equals(id2));
        }

        [Test]
        public void NetworkBehaviourIdNotEqualsNetID()
        {
            var id1 = new NetworkBehaviour.Id(10, 2);
            var id2 = new NetworkBehaviour.Id(11, 2);

            Assert.IsFalse(id1.Equals(id2));
        }

        [Test]
        public void NetworkBehaviourIdNotEqualsCompIndex()
        {
            var id1 = new NetworkBehaviour.Id(10, 2);
            var id2 = new NetworkBehaviour.Id(10, 1);

            Assert.IsFalse(id1.Equals(id2));
        }

        [Test]
        public void HasIdentitysOwner()
        {
            (_, hostIdentity.Owner) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            Assert.That(hostComponent.Owner, Is.EqualTo(hostIdentity.Owner));
        }

        [Test]
        public void ComponentIndex()
        {
            var extraObject = CreateNetworkIdentity();

            var behaviour1 = extraObject.gameObject.AddComponent<SampleBehavior>();
            var behaviour2 = extraObject.gameObject.AddComponent<SampleBehavior>();

            // original one is first networkbehaviour, so index is 0
            Assert.That(behaviour1.ComponentIndex, Is.EqualTo(0));
            // extra one is second networkbehaviour, so index is 1
            Assert.That(behaviour2.ComponentIndex, Is.EqualTo(1));
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourHookGuardTester : NetworkBehaviour
    {
        [Test]
        public void HookGuard()
        {
            // set hook guard for some bits
            for (var i = 0; i < 10; ++i)
            {
                var bit = 1ul << i;

                // should be false by default
                Assert.That(GetSyncVarHookGuard(bit), Is.False);

                // set true
                SetSyncVarHookGuard(bit, true);
                Assert.That(GetSyncVarHookGuard(bit), Is.True);

                // set false again
                SetSyncVarHookGuard(bit, false);
                Assert.That(GetSyncVarHookGuard(bit), Is.False);
            }
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourInitSyncObjectTester : NetworkBehaviour
    {
        [Test]
        public void InitSyncObject()
        {
            ISyncObject syncObject = new SyncList<bool>();
            InitSyncObject(syncObject);
            Assert.That(syncObjects.Count, Is.EqualTo(1));
            Assert.That(syncObjects[0], Is.EqualTo(syncObject));
        }
    }
}
