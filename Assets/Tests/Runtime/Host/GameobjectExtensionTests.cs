using System;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host
{
    public class GameobjectExtensionTests : HostSetup<MockComponent>
    {
        [Test]
        public void GetNetworkIdentity()
        {
            Assert.That(hostPlayerGO.GetNetworkIdentity(), Is.EqualTo(hostIdentity));
        }

        [Test]
        public void GetNoNetworkIdentity()
        {
            // create a GameObject without NetworkIdentity
            var goWithout = CreateGameObject();

            // GetNetworkIdentity for GO without identity
            // (error log is expected)
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = goWithout.GetNetworkIdentity();
            });
        }
    }
}
