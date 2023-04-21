using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Components
{
    public class NetworkManagerHudTestSetup : ServerSetup
    {
        // set to for host mode, but dont start anything
        // this will allow the hud to then control it
        protected override bool StartServer => false;
        protected override bool HostMode => true;

        private NetworkManagerHud _hud;
        private NetworkManager _manager;

        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);

            _manager = instance.GameObject.AddComponent<NetworkManager>();
            _manager.Client = instance.Client;
            _manager.Server = server;

            _hud = CreateMonoBehaviour<NetworkManagerHud>();
            _hud.NetworkManager = _manager;
            _hud.OfflineGO = CreateGameObject();
            _hud.OnlineGO = CreateGameObject();

            //Initial state in the prefab
            _hud.OfflineGO.SetActive(true);
            _hud.OnlineGO.SetActive(false);
        }

        [Test]
        public void StartClientButtonChangingActiveObject()
        {
            _hud.StartClientButtonHandler();
            Assert.That(_hud.OfflineGO.activeSelf, Is.False);
            Assert.That(_hud.OnlineGO.activeSelf, Is.True);
        }

        [Test]
        public void OnlineSetActiveTest()
        {
            _hud.OnlineSetActive();
            Assert.That(_hud.OfflineGO.activeSelf, Is.False);
            Assert.That(_hud.OnlineGO.activeSelf, Is.True);
        }

        [Test]
        public void OfflineSetActiveTest()
        {
            _hud.OfflineSetActive();
            Assert.That(_hud.OfflineGO.activeSelf, Is.True);
            Assert.That(_hud.OnlineGO.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator StopButtonTest()
        {
            _hud.StartHostButtonHandler();

            // should start within 1 frame
            yield return null;
            yield return null;

            _hud.StopButtonHandler();
            Assert.That(_hud.OfflineGO.activeSelf, Is.True);
            Assert.That(_hud.OnlineGO.activeSelf, Is.False);

            // should disable within 1 frame
            yield return null;

            Assert.That(_manager.IsNetworkActive, Is.False);
        }

        [Test]
        public void StartServerOnlyButtonTest()
        {
            _hud.StartServerOnlyButtonHandler();
            Assert.That(_hud.OfflineGO.activeSelf, Is.False);
            Assert.That(_hud.OnlineGO.activeSelf, Is.True);

            Assert.That(_manager.Server.Active, Is.True);
        }
    }
}
