using Mirage.Experimental;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public class NetworkRigidbodyTest : HostSetup<NetworkRigidbody>
    {
        [Test]
        public void InitsyncVelocityTest()
        {
            Assert.That(playerComponent.syncVelocity, Is.True);
        }

        [Test]
        public void InitvelocitySensitivityTest()
        {
            Assert.That(playerComponent.velocitySensitivity, Is.InRange(0.001f, 0.199f));
        }

        [Test]
        public void InitsyncAngularVelocityTest()
        {
            Assert.That(playerComponent.syncAngularVelocity, Is.True);
        }

        [Test]
        public void InitangularVelocitySensitivityTest()
        {
            Assert.That(playerComponent.angularVelocitySensitivity, Is.InRange(0.001f, 0.199f));
        }
    }
}
