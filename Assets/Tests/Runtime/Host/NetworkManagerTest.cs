using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public class NetworkManagerTest : HostSetup
    {
        private NetworkManager manager;

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            manager = serverGo.AddComponent<NetworkManager>();
            manager.Server = server;
            manager.ServerObjectManager = serverObjectManager;
            manager.Client = client;
            manager.ClientObjectManager = clientObjectManager;
        }

        [Test]
        public void IsNetworkActiveTest()
        {
            Assert.That(manager.IsNetworkActive, Is.True);
        }

        [UnityTest]
        public IEnumerator IsNetworkActiveStopTest() => UniTask.ToCoroutine(async () =>
        {
            manager.Server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !client.Active);

            Assert.That(server.Active, Is.False);
            Assert.That(client.Active, Is.False);
            Assert.That(manager.IsNetworkActive, Is.False);
        });

        [UnityTest]
        public IEnumerator StopClientTest() => UniTask.ToCoroutine(async () =>
        {
            manager.Client.Disconnect();

            await AsyncUtil.WaitUntilWithTimeout(() => !client.Active);
        });

        [Test]
        public void NetworkManagerModeHostTest()
        {
            Assert.That(manager.NetworkMode == NetworkManagerMode.Host);
        }

        [UnityTest]
        public IEnumerator NetworkManagerModeOfflineHostTest() => UniTask.ToCoroutine(async () =>
        {
            server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active && !client.Active);

            Assert.That(manager.NetworkMode == NetworkManagerMode.None);
        });
    }
}
