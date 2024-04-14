using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using Mirage.Tests.Runtime.Syncing;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class SampleBehaviorWithNB : NetworkBehaviour
    {
        [SyncVar]
        public SampleBehaviorWithNB target;
    }

    public class NetworkBehaviorSyncvarTest : ClientServerSetup<SampleBehaviorWithNB>
    {
        private NetworkWriter writer;
        private MirageNetworkReader reader;

        protected override UniTask ExtraSetup()
        {
            writer = new NetworkWriter(1200);
            reader = new MirageNetworkReader();
            return base.ExtraSetup();
        }
        public override void ExtraTearDown()
        {
            reader?.Dispose();
        }


        [Test]
        public void IsNullByDefault()
        {
            // out of the box, target should be null in the client

            Assert.That(clientComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ChangeTarget() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.target = serverComponent;

            await UniTask.WaitUntil(() => clientComponent.target != null);

            Assert.That(clientComponent.target, Is.SameAs(clientComponent));
        });

        [UnityTest]
        public IEnumerator SpawnWithTarget() => UniTask.ToCoroutine(async () =>
        {
            // create an object, set the target and spawn it
            var newObject = InstantiateForTest(_characterPrefabGo);
            var newBehavior = newObject.GetComponent<SampleBehaviorWithNB>();
            newBehavior.target = serverComponent;
            serverObjectManager.Spawn(newObject);

            // wait until the client spawns it
            var newObjectId = newBehavior.NetId;
            var newClientObject = await AsyncUtil.WaitUntilSpawn(client.World, newObjectId);

            // check if the target was set correctly in the client
            var newClientBehavior = newClientObject.GetComponent<SampleBehaviorWithNB>();
            Assert.That(newClientBehavior.target, Is.SameAs(clientComponent));

            // cleanup
            serverObjectManager.Destroy(newObject);
        });

        [Test]
        public void NetworkBehaviorSyncvarGetOnClient()
        {
            var goSyncvar = new NetworkBehaviorSyncvar
            {
                _objectLocator = client.World,
                _netId = serverIdentity.NetId,
                _component = null,
            };

            Assert.That(goSyncvar.Value, Is.SameAs(clientComponent));
        }
        [Test]
        public void NetworkBehaviorSyncvarGetOnClientGeneric()
        {
            var goSyncvar = (NetworkBehaviorSyncvar<SampleBehaviorWithNB>)new NetworkBehaviorSyncvar
            {
                _objectLocator = client.World,
                _netId = serverIdentity.NetId,
                _component = null,
            };

            Assert.That(goSyncvar.Value, Is.TypeOf<SampleBehaviorWithNB>());
            Assert.That(goSyncvar.Value, Is.SameAs(clientComponent));
        }
        [Test]
        public void NetworkBehaviorSyncvarSync()
        {
            var serverValue = new NetworkBehaviorSyncvar(serverComponent);
            writer.Write(serverValue);
            reader.ObjectLocator = client.World;
            reader.Reset(writer.ToArraySegment());
            var clientValue = reader.Read<NetworkBehaviorSyncvar>();

            Assert.That(clientValue.Value, Is.SameAs(clientComponent));
        }
        [Test]
        public void NetworkBehaviorSyncvarSyncGeneric()
        {
            var serverValue = new NetworkBehaviorSyncvar<SampleBehaviorWithNB>(serverComponent);
            writer.Write(serverValue);
            reader.ObjectLocator = client.World;
            reader.Reset(writer.ToArraySegment());
            var clientValue = reader.Read<NetworkBehaviorSyncvar<SampleBehaviorWithNB>>();

            Assert.That(clientValue.Value, Is.TypeOf<SampleBehaviorWithNB>());
            Assert.That(clientValue.Value, Is.SameAs(clientComponent));
        }
        [Test]
        public void GetAs()
        {
            var serverValue = new NetworkBehaviorSyncvar(serverComponent);
            var value = serverValue.GetAs<SampleBehaviorWithNB>();

            Assert.That(value, Is.TypeOf<SampleBehaviorWithNB>());
            Assert.That(value, Is.SameAs(serverComponent));
        }

        [Test]
        public void GetAsThrow()
        {
            var serverValue = new NetworkBehaviorSyncvar(serverComponent);

            Assert.Throws<InvalidCastException>(() =>
            {
                var value = serverValue.GetAs<MockPlayer>();
            });
        }
    }
}
