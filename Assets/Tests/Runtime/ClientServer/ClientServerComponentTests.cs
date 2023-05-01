using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;
using Guid = System.Guid;
using Random = UnityEngine.Random;

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

        [Test]
        public void ServerRpcWithSenderOnServer()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverComponent.SendWithSender(1);
            });

            Assert.That(exception, Has.Message.EqualTo("Server RPC can only be called when client is active"));
        }

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
            var spawnDelegateCalled = 0;
            var hash = Guid.NewGuid().GetHashCode();

            var identity = CreateNetworkIdentity();
            identity.PrefabHash = hash;
            identity.NetId = (uint)Random.Range(0, int.MaxValue);

            SpawnHandlerDelegate spawnDelegate = (msg) =>
            {
                spawnDelegateCalled++;
                return InstantiateForTest(identity);
            };
            clientObjectManager.RegisterSpawnHandler(hash, spawnDelegate, go => { });
            serverObjectManager.SendSpawnMessage(identity, serverPlayer);

            await AsyncUtil.WaitUntilWithTimeout(() => spawnDelegateCalled != 0);

            Assert.That(spawnDelegateCalled, Is.EqualTo(1));
        });

        [UnityTest]
        public IEnumerator OnDestroySpawnHandlerTest() => UniTask.ToCoroutine(async () =>
        {
            var spawnDelegateCalled = 0;
            var hash = Guid.NewGuid().GetHashCode();
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = hash;
            identity.NetId = (uint)Random.Range(0, int.MaxValue);

            SpawnHandlerDelegate spawnDelegate = (msg) =>
            {
                spawnDelegateCalled++;
                return InstantiateForTest(identity);
            };
            var unspawnDelegate = Substitute.For<UnSpawnDelegate>();

            clientObjectManager.RegisterSpawnHandler(hash, spawnDelegate, unspawnDelegate);
            serverObjectManager.SendSpawnMessage(identity, serverPlayer);

            await AsyncUtil.WaitUntilWithTimeout(() => spawnDelegateCalled != 0);

            clientObjectManager.OnObjectDestroy(new ObjectDestroyMessage
            {
                NetId = identity.NetId
            });
            unspawnDelegate.Received().Invoke(Arg.Any<NetworkIdentity>());
        });

        [UnityTest]
        public IEnumerator ClientDisconnectTest() => UniTask.ToCoroutine(async () =>
        {
            var playerCount = server.Players.Count;
            client.Disconnect();

            await AsyncUtil.WaitUntilWithTimeout(() => client._connectState == ConnectState.Disconnected);
            // player could should be 1 less after client disconnects
            await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count == playerCount - 1);
        });
    }
}
