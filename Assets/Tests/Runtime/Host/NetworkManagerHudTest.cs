using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public abstract class NetworkManagerHudTestSetup : HostSetup<MockComponent>
    {
        protected NetworkManagerHud networkManagerHud;

        public override void ExtraSetup()
        {
            networkManagerHud = CreateMonoBehaviour<NetworkManagerHud>();
            networkManagerHud.NetworkManager = manager;
            networkManagerHud.OfflineGO = CreateGameObject();
            networkManagerHud.OnlineGO = CreateGameObject();

            //Initial state in the prefab
            networkManagerHud.OfflineGO.SetActive(true);
            networkManagerHud.OnlineGO.SetActive(false);
        }
    }

    [TestFixture]
    public class NetworkManagerHudTest : NetworkManagerHudTestSetup
    {
        [Test]
        public void OnlineSetActiveTest()
        {
            networkManagerHud.OnlineSetActive();
            Assert.That(networkManagerHud.OfflineGO.activeSelf, Is.False);
            Assert.That(networkManagerHud.OnlineGO.activeSelf, Is.True);
        }

        [Test]
        public void OfflineSetActiveTest()
        {
            networkManagerHud.OfflineSetActive();
            Assert.That(networkManagerHud.OfflineGO.activeSelf, Is.True);
            Assert.That(networkManagerHud.OnlineGO.activeSelf, Is.False);
        }



        [UnityTest]
        public IEnumerator StopButtonTest() => UniTask.ToCoroutine(async () =>
        {
            networkManagerHud.StopButtonHandler();
            Assert.That(networkManagerHud.OfflineGO.activeSelf, Is.True);
            Assert.That(networkManagerHud.OnlineGO.activeSelf, Is.False);

            await AsyncUtil.WaitUntilWithTimeout(() => !manager.IsNetworkActive);

            Assert.That(manager.IsNetworkActive, Is.False);
        });
    }

    [TestFixture]
    public class NetworkManagerHudTestNoAutoStart : NetworkManagerHudTestSetup
    {
        protected override bool AutoStartServer => false;

        [Test]
        public void StartServerOnlyButtonTest()
        {
            networkManagerHud.StartServerOnlyButtonHandler();
            Assert.That(networkManagerHud.OfflineGO.activeSelf, Is.False);
            Assert.That(networkManagerHud.OnlineGO.activeSelf, Is.True);

            Assert.That(manager.Server.Active, Is.True);
        }
    }
}
