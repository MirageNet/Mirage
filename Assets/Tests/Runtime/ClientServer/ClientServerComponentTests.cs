using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Guid = System.Guid;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ClientServerComponentTests : ClientServerSetup<MockComponent>
    {
        [Test]
        public void CheckNotHost()
        {
            Assert.That(serverPlayerGO, Is.Not.SameAs(clientPlayerGO));

            Assert.That(serverPlayerGO, Is.Not.Null);
            Assert.That(clientPlayerGO, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ServerRpc() => UniTask.ToCoroutine(async () =>
        {
            clientComponent.Send2Args(1, "hello");

            await AsyncUtil.WaitUntilWithTimeout(() => serverComponent.cmdArg1 != 0);

            Assert.That(serverComponent.cmdArg1, Is.EqualTo(1));
            Assert.That(serverComponent.cmdArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ServerRpcWithSenderOnClient() => UniTask.ToCoroutine(async () =>
        {
            clientComponent.SendWithSender(1);

            await AsyncUtil.WaitUntilWithTimeout(() => serverComponent.cmdArg1 != 0);

            Assert.That(serverComponent.cmdArg1, Is.EqualTo(1));
            Assert.That(serverComponent.cmdSender, Is.EqualTo(serverPlayer), "ServerRpc called on client will have client's player (server version)");
        });

        [UnityTest]
        public IEnumerator ServerRpcWithSenderOnServer() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.SendWithSender(1);

            await AsyncUtil.WaitUntilWithTimeout(() => serverComponent.cmdArg1 != 0);

            Assert.That(serverComponent.cmdArg1, Is.EqualTo(1));
            Assert.That(serverComponent.cmdSender, Is.Null, "ServerRPC called on server will have no sender");
        });

        [UnityTest]
        public IEnumerator ServerRpcWithNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            clientComponent.CmdNetworkIdentity(clientIdentity);

            await AsyncUtil.WaitUntilWithTimeout(() => serverComponent.cmdNi != null);

            Assert.That(serverComponent.cmdNi, Is.SameAs(serverIdentity));
        });

        [UnityTest]
        public IEnumerator ClientRpc() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.RpcTest(1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => clientComponent.rpcArg1 != 0);

            Assert.That(clientComponent.rpcArg1, Is.EqualTo(1));
            Assert.That(clientComponent.rpcArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ClientConnRpc() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.ClientConnRpcTest(serverPlayer, 1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => clientComponent.targetRpcArg1 != 0);

            Assert.That(clientComponent.targetRpcPlayer, Is.EqualTo(clientPlayer));
            Assert.That(clientComponent.targetRpcArg1, Is.EqualTo(1));
            Assert.That(clientComponent.targetRpcArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ClientOwnerRpc() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.RpcOwnerTest(1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => clientComponent.rpcOwnerArg1 != 0);

            Assert.That(clientComponent.rpcOwnerArg1, Is.EqualTo(1));
            Assert.That(clientComponent.rpcOwnerArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator OnSpawnSpawnHandlerTest() => UniTask.ToCoroutine(async () =>
        {
            spawnDelegateTestCalled = 0;
            int hash = Guid.NewGuid().GetHashCode();
            var gameObject = new GameObject();
            NetworkIdentity identity = gameObject.AddComponent<NetworkIdentity>();
            identity.PrefabHash = hash;
            identity.NetId = (uint)Random.Range(0, int.MaxValue);

            clientObjectManager.RegisterSpawnHandler(hash, SpawnDelegateTest, go => { });
            clientObjectManager.RegisterPrefab(identity, hash);
            serverObjectManager.SendSpawnMessage(identity, serverPlayer);

            await AsyncUtil.WaitUntilWithTimeout(() => spawnDelegateTestCalled != 0);

            Assert.That(spawnDelegateTestCalled, Is.EqualTo(1));
        });

        [UnityTest]
        public IEnumerator OnDestroySpawnHandlerTest() => UniTask.ToCoroutine(async () =>
        {
            spawnDelegateTestCalled = 0;
            int hash = Guid.NewGuid().GetHashCode();
            var gameObject = new GameObject();
            NetworkIdentity identity = gameObject.AddComponent<NetworkIdentity>();
            identity.PrefabHash = hash;
            identity.NetId = (uint)Random.Range(0, int.MaxValue);

            UnSpawnDelegate unspawnDelegate = Substitute.For<UnSpawnDelegate>();

            clientObjectManager.RegisterSpawnHandler(hash, SpawnDelegateTest, unspawnDelegate);
            clientObjectManager.RegisterPrefab(identity, hash);
            serverObjectManager.SendSpawnMessage(identity, serverPlayer);

            await AsyncUtil.WaitUntilWithTimeout(() => spawnDelegateTestCalled != 0);

            clientObjectManager.OnObjectDestroy(new ObjectDestroyMessage
            {
                netId = identity.NetId
            });
            unspawnDelegate.Received().Invoke(Arg.Any<NetworkIdentity>());
        });

        private int spawnDelegateTestCalled;

        private NetworkIdentity SpawnDelegateTest(SpawnMessage msg)
        {
            spawnDelegateTestCalled++;

            NetworkIdentity prefab = clientObjectManager.GetPrefab(msg.prefabHash.Value);
            if (!(prefab is null))
            {
                return Object.Instantiate(prefab);
            }
            return null;
        }

        [UnityTest]
        public IEnumerator ClientDisconnectTest() => UniTask.ToCoroutine(async () =>
        {
            int playerCount = server.Players.Count;
            client.Disconnect();

            await AsyncUtil.WaitUntilWithTimeout(() => client.connectState == ConnectState.Disconnected);
            // player could should be 1 less after client disconnects
            await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count == playerCount - 1);
        });
    }
}
