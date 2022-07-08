using Mirage.Collections;
using NUnit.Framework;
using UnityEngine;

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
            Assert.That(playerComponent.IsServerOnly, Is.False);
        }

        [Test]
        public void IsServer()
        {
            Assert.That(playerComponent.IsServer, Is.True);
        }

        [Test]
        public void IsClient()
        {
            Assert.That(playerComponent.IsClient, Is.True);
        }

        [Test]
        public void IsClientOnly()
        {
            Assert.That(playerComponent.IsClientOnly, Is.False);
        }

        [Test]
        public void PlayerHasAuthorityByDefault()
        {
            // no authority by default
            Assert.That(playerComponent.HasAuthority, Is.True);
        }

        class OnStartServerTestComponent : NetworkBehaviour
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
            GameObject gameObject = CreateGameObject();
            NetworkIdentity netIdentity = gameObject.AddComponent<NetworkIdentity>();
            OnStartServerTestComponent comp = gameObject.AddComponent<OnStartServerTestComponent>();
            netIdentity.OnStartServer.AddListener(comp.OnStartServer);

            Assert.That(comp.called, Is.False);
            serverObjectManager.Spawn(gameObject);

            Assert.That(comp.called, Is.True);
        }

        [Test]
        public void SpawnedObjectNoAuthority()
        {
            SampleBehavior behaviour = CreateBehaviour<SampleBehavior>();
            serverObjectManager.Spawn(behaviour.gameObject);

            client.Update();

            // no authority by default
            Assert.That(behaviour.HasAuthority, Is.False);
        }

        [Test]
        public void HasIdentitysNetId()
        {
            playerIdentity.NetId = 42;
            Assert.That(playerComponent.NetId, Is.EqualTo(42));
        }

        [Test]
        public void HasIdentitysOwner()
        {
            (_, playerIdentity.Owner) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            Assert.That(playerComponent.Owner, Is.EqualTo(playerIdentity.Owner));
        }

        [Test]
        public void ComponentIndex()
        {
            NetworkIdentity extraObject = CreateNetworkIdentity();

            SampleBehavior behaviour1 = extraObject.gameObject.AddComponent<SampleBehavior>();
            SampleBehavior behaviour2 = extraObject.gameObject.AddComponent<SampleBehavior>();

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
            for (int i = 0; i < 10; ++i)
            {
                ulong bit = 1ul << i;

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
