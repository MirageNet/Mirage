using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using static Mirror.Tests.AsyncUtil;
using Object = UnityEngine.Object;

namespace Mirror.Tests
{
    [TestFixture]
    public class NetworkManagerHudTest : HostSetup<MockComponent>
    {
        GameObject gameObject;
        NetworkManagerHUD networkManagerHUD;
        public override void ExtraSetup()
        {
            gameObject = new GameObject("NetworkManagerHUD", typeof(NetworkManagerHUD));
            networkManagerHUD = gameObject.GetComponent<NetworkManagerHUD>();
            networkManagerHUD.NetworkManager = manager;
            networkManagerHUD.OfflineGO = new GameObject();
            networkManagerHUD.OnlineGO = new GameObject();

            //Initial state in the prefab
            networkManagerHUD.OfflineGO.SetActive(true);
            networkManagerHUD.OnlineGO.SetActive(false);
            //networkManagerHUD.NetworkAddressInput.text = "localhost";
        }

        public override void ExtraTearDown()
        {
            Object.DestroyImmediate(networkManagerHUD.OfflineGO);
            Object.DestroyImmediate(networkManagerHUD.OnlineGO);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void OnlineSetActiveTest()
        {
            networkManagerHUD.OnlineSetActive();
            Assert.That(networkManagerHUD.OfflineGO.activeSelf, Is.False);
            Assert.That(networkManagerHUD.OnlineGO.activeSelf, Is.True);
        }

        [Test]
        public void OfflineSetActiveTest()
        {
            networkManagerHUD.OfflineSetActive();
            Assert.That(networkManagerHUD.OfflineGO.activeSelf, Is.True);
            Assert.That(networkManagerHUD.OnlineGO.activeSelf, Is.False);
        }

        [Test]
        public void StartHostButtonTest()
        {
            networkManagerHUD.StartHostButtonHandler();
            Assert.That(networkManagerHUD.OfflineGO.activeSelf, Is.False);
            Assert.That(networkManagerHUD.OnlineGO.activeSelf, Is.True);

            Assert.That(manager.server.Active, Is.True);
            Assert.That(manager.client.Active, Is.True);
        }

        [Test]
        public void StartServerOnlyButtonTest()
        {
            networkManagerHUD.StartServerOnlyButtonHandler();
            Assert.That(networkManagerHUD.OfflineGO.activeSelf, Is.False);
            Assert.That(networkManagerHUD.OnlineGO.activeSelf, Is.True);

            Assert.That(manager.server.Active, Is.True);
        }

        [UnityTest]
        public IEnumerator StopButtonTest() => RunAsync(async () =>
        {
            networkManagerHUD.StopButtonHandler();
            Assert.That(networkManagerHUD.OfflineGO.activeSelf, Is.True);
            Assert.That(networkManagerHUD.OnlineGO.activeSelf, Is.False);

            await WaitFor(() => !manager.IsNetworkActive);

            Assert.That(manager.IsNetworkActive, Is.False);
        });
    }

    [TestFixture]
    public class NetworkManagerHudClientServerTest : ClientServerSetup<MockComponent>
    {
        GameObject gameObject;
        NetworkManagerHUD networkManagerHUD;
        public override void ExtraSetup()
        {
            gameObject = new GameObject("NetworkManagerHUD", typeof(NetworkManagerHUD));
            networkManagerHUD = gameObject.GetComponent<NetworkManagerHUD>();
            clientGo.AddComponent<MockTransport>();
            networkManagerHUD.NetworkManager = clientGo.AddComponent<NetworkManager>();
            networkManagerHUD.OfflineGO = new GameObject();
            networkManagerHUD.OnlineGO = new GameObject();

            //Initial state in the prefab
            networkManagerHUD.OfflineGO.SetActive(true);
            networkManagerHUD.OnlineGO.SetActive(false);
        }

        public override void ExtraTearDown()
        {
            Object.DestroyImmediate(networkManagerHUD.OfflineGO);
            Object.DestroyImmediate(networkManagerHUD.OnlineGO);
            Object.DestroyImmediate(gameObject);
        }

        //[Test]
        //public void StartClientButtonTest()
        //{
            //TODO:
            //StartClientButtonTest (0.123s)
            //---
            //System.NullReferenceException : Object reference not set to an instance of an object
            //---
            //at Mirror.NetworkManager.StartClient(System.String serverIp)[0x00025] in F:\Dev\MirrorNG\Assets\Mirror\Runtime\NetworkManager.cs:65
            //at Mirror.NetworkManagerHUD.StartClientButtonHandler()[0x00001] in F:\Dev\MirrorNG\Assets\Mirror\Runtime\NetworkManagerHUD.cs:43

            //networkManagerHUD.StartClientButtonHandler();
            //Assert.That(networkManagerHUD.OfflineGO.activeSelf, Is.False);
            //Assert.That(networkManagerHUD.OnlineGO.activeSelf, Is.True);
        //}
    }
}
