using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    [TestFixture]
    public class NetworkManagerHudClientServerTest : ClientServerSetup<MockComponent>
    {
        protected override bool AutoConnectClient => false;

        private NetworkManagerHud networkManagerHud;
        public override void ExtraSetup()
        {
            networkManagerHud = CreateMonoBehaviour<NetworkManagerHud>();
            networkManagerHud.NetworkManager = clientGo.AddComponent<NetworkManager>();
            networkManagerHud.NetworkManager.Client = client;
            networkManagerHud.OfflineGO = CreateGameObject();
            networkManagerHud.OnlineGO = CreateGameObject();

            //Initial state in the prefab
            networkManagerHud.OfflineGO.SetActive(true);
            networkManagerHud.OnlineGO.SetActive(false);
        }

        [Test]
        public void StartClientButtonTest()
        {
            networkManagerHud.StartClientButtonHandler();
            Assert.That(networkManagerHud.OfflineGO.activeSelf, Is.False);
            Assert.That(networkManagerHud.OnlineGO.activeSelf, Is.True);
        }
    }
}
