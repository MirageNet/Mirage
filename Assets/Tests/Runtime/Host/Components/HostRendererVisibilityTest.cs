using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Host
{
    public class HostRendererVisibilityTest : HostSetup<MockVisibility>
    {
        private MockVisibility _visibility;
        private Renderer _renderer;

        public override UniTask LateSetup()
        {
            _visibility = CreateBehaviour<MockVisibility>();
            _renderer = _visibility.gameObject.AddComponent<MeshRenderer>();
            // add 2nd so that awake finds renderer
            _visibility.gameObject.AddComponent<HostRendererVisibility>();
            Debug.Assert(_renderer.enabled, "should start enabled");
            serverObjectManager.Spawn(_visibility.Identity);

            return UniTask.CompletedTask;
        }

        [Test]
        public void ShouldDisableOnSpawnIfNotVisible()
        {
            Assert.That(_renderer.enabled, Is.False);
        }

        [Test]
        public void ShouldEnableWhenBecomesVisible()
        {
            _visibility.Visible = true;
            Assert.That(_renderer.enabled, Is.True);
        }

        [Test]
        public void ShouldDisableWhenBecomesHidden()
        {
            _visibility.Visible = true;
            _visibility.Visible = false;
            Assert.That(_renderer.enabled, Is.False);
        }
    }
}
