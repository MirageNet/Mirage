using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.RemoteCalls;
using Mirage.Tests.Runtime.RpcTests.Async;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.RpcTests
{
    public class MultiInstanceRpcRoutingTests : MultiRemoteClientSetup
    {
        protected override int RemoteClientCount => 2;

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            base.ExtraPrefabSetup(prefab);
            prefab.gameObject.AddComponent<MockRpcComponent>();
            prefab.gameObject.AddComponent<ReturnRpcComponent_int>();
        }

        private MockRpcComponent ServerMock(int i) => ServerGameObject(i).GetComponent<MockRpcComponent>();
        private MockRpcComponent ClientMock(int i) => ClientPlayerGO(i).GetComponent<MockRpcComponent>();
        private ReturnRpcComponent_int ServerReturn(int i) => ServerGameObject(i).GetComponent<ReturnRpcComponent_int>();
        private ReturnRpcComponent_int ClientReturn(int i) => ClientPlayerGO(i).GetComponent<ReturnRpcComponent_int>();

        [Test]
        public void IdentitiesShareCachedRemoteCallCollection()
        {
            Assert.That(ServerIdentity(0).RemoteCallCollection, Is.Not.Null);
            Assert.That(ServerIdentity(1).RemoteCallCollection, Is.SameAs(ServerIdentity(0).RemoteCallCollection));
            Assert.That(ClientIdentity(0).RemoteCallCollection, Is.SameAs(ClientIdentity(1).RemoteCallCollection));
        }

        [Test]
        public void CacheReturnsEmptyForZeroBehaviours()
        {
            var collection = RemoteCallCollectionCache.GetOrCreate(Array.Empty<NetworkBehaviour>());
            Assert.That(collection, Is.SameAs(RemoteCallCollection.Empty));
        }

        [Test]
        public void CacheDistinguishesDifferentComponentLayouts()
        {
            var go1 = new GameObject();
            go1.AddComponent<NetworkIdentity>();
            var comp1 = go1.AddComponent<MockRpcComponent>();

            var go2 = new GameObject();
            go2.AddComponent<NetworkIdentity>();
            var comp2 = go2.AddComponent<ReturnRpcComponent_int>();

            var go3 = new GameObject();
            go3.AddComponent<NetworkIdentity>();
            var comp3A = go3.AddComponent<MockRpcComponent>();
            var comp3B = go3.AddComponent<ReturnRpcComponent_int>();

            var layout1 = RemoteCallCollectionCache.GetOrCreate(new NetworkBehaviour[] { comp1 });
            var layout2 = RemoteCallCollectionCache.GetOrCreate(new NetworkBehaviour[] { comp2 });
            var layoutCombined = RemoteCallCollectionCache.GetOrCreate(new NetworkBehaviour[] { comp3A, comp3B });

            Assert.That(layout1, Is.Not.SameAs(layout2));
            Assert.That(layout1, Is.Not.SameAs(layoutCombined));

            UnityEngine.Object.DestroyImmediate(go1);
            UnityEngine.Object.DestroyImmediate(go2);
            UnityEngine.Object.DestroyImmediate(go3);
        }

        [UnityTest]
        public IEnumerator ServerRpcRoutesToCorrectInstance()
        {
            ClientMock(0).Server2Args(10, "instanceA");
            ClientMock(1).Server2Args(20, "instanceB");

            yield return null;
            yield return null;

            Assert.That(ServerMock(0).Server2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(ServerMock(0).Server2ArgsCalls[0].arg1, Is.EqualTo(10));
            Assert.That(ServerMock(0).Server2ArgsCalls[0].arg2, Is.EqualTo("instanceA"));

            Assert.That(ServerMock(1).Server2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(ServerMock(1).Server2ArgsCalls[0].arg1, Is.EqualTo(20));
            Assert.That(ServerMock(1).Server2ArgsCalls[0].arg2, Is.EqualTo("instanceB"));
        }

        [UnityTest]
        public IEnumerator ClientRpcRoutesToCorrectInstance()
        {
            ServerMock(0).Client2Args(100, "clientMsgA");
            ServerMock(1).Client2Args(200, "clientMsgB");

            yield return null;
            yield return null;

            Assert.That(ClientMock(0).Client2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(ClientMock(0).Client2ArgsCalls[0].arg1, Is.EqualTo(100));
            Assert.That(ClientMock(0).Client2ArgsCalls[0].arg2, Is.EqualTo("clientMsgA"));

            Assert.That(ClientMock(1).Client2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(ClientMock(1).Client2ArgsCalls[0].arg1, Is.EqualTo(200));
            Assert.That(ClientMock(1).Client2ArgsCalls[0].arg2, Is.EqualTo("clientMsgB"));
        }

        [UnityTest]
        public IEnumerator RequestServerRpcReturnsCorrectInstanceResult() => UniTask.ToCoroutine(async () =>
        {
            ServerReturn(0).rpcResult = 111;
            ServerReturn(1).rpcResult = 222;

            var taskA = ClientReturn(0).GetResultServer();
            var taskB = ClientReturn(1).GetResultServer();

            var resultA = await taskA;
            var resultB = await taskB;

            Assert.That(resultA, Is.EqualTo(111));
            Assert.That(resultB, Is.EqualTo(222));
        });
    }
}
