using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;
using Guid = System.Guid;
using Random = UnityEngine.Random;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ClientServerComponentTests : ClientServerSetup
    {
        [Test]
        public void CheckNotHost()
        {
            Assert.That(serverPlayerGO, Is.Not.SameAs(clientPlayerGO));

            Assert.That(serverPlayerGO, Is.Not.Null);
            Assert.That(clientPlayerGO, Is.Not.Null);
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
            var playerCount = server.AllPlayers.Count;
            client.Disconnect();

            await AsyncUtil.WaitUntilWithTimeout(() => client._connectState == ConnectState.Disconnected);
            // player could should be 1 less after client disconnects
            await AsyncUtil.WaitUntilWithTimeout(() => server.AllPlayers.Count == playerCount - 1);
        });
    }
}
