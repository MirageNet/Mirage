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
        public IEnumerator ServerRpc()
        {
            clientComponent.Server2Args(1, "hello");

            yield return null;
            yield return null;

            Assert.That(serverComponent.Server2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(serverComponent.Server2ArgsCalls[0].arg1, Is.EqualTo(1));
            Assert.That(serverComponent.Server2ArgsCalls[0].arg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ServerRpcWithSenderOnClient()
        {
            clientComponent.ServerWithSender(1);

            yield return null;
            yield return null;

            Assert.That(serverComponent.ServerWithSenderCalls.Count, Is.EqualTo(1));
            Assert.That(serverComponent.ServerWithSenderCalls[0].arg1, Is.EqualTo(1));
            Assert.That(serverComponent.ServerWithSenderCalls[0].sender, Is.EqualTo(serverPlayer));
        }

        [Test]
        public void ServerRpcWithSenderOnServer()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverComponent.ServerWithSender(1);
            });

            Assert.That(exception, Has.Message.EqualTo("Server RPC can only be called when client is active"));
        }

        [UnityTest]
        public IEnumerator ServerRpcWithNetworkIdentity()
        {
            clientComponent.ServerWithNI(clientIdentity);

            yield return null;
            yield return null;

            Assert.That(serverComponent.ServerWithNICalls.Count, Is.EqualTo(1));
            Assert.That(serverComponent.ServerWithNICalls[0], Is.SameAs(serverIdentity));
        }

        [UnityTest]
        public IEnumerator ClientRpc()
        {
            serverComponent.Client2Args(1, "hello");

            yield return null;
            yield return null;

            Assert.That(clientComponent.Client2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(clientComponent.Client2ArgsCalls[0].arg1, Is.EqualTo(1));
            Assert.That(clientComponent.Client2ArgsCalls[0].arg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ClientConnRpc()
        {
            serverComponent.ClientTarget(serverPlayer, 1, "hello");

            yield return null;
            yield return null;

            Assert.That(clientComponent.ClientTargetCalls.Count, Is.EqualTo(1));
            Assert.That(clientComponent.ClientTargetCalls[0].player, Is.EqualTo(clientPlayer));
            Assert.That(clientComponent.ClientTargetCalls[0].arg1, Is.EqualTo(1));
            Assert.That(clientComponent.ClientTargetCalls[0].arg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ClientOwnerRpc()
        {
            serverComponent.ClientOwner(1, "hello");

            yield return null;
            yield return null;

            Assert.That(clientComponent.ClientOwnerCalls.Count, Is.EqualTo(1));
            Assert.That(clientComponent.ClientOwnerCalls[0].arg1, Is.EqualTo(1));
            Assert.That(clientComponent.ClientOwnerCalls[0].arg2, Is.EqualTo("hello"));
        }

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
