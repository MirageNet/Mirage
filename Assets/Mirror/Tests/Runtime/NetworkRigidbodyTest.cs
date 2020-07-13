using Mirror.Experimental;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class NetworkRigidbodyTest : HostSetup<NetworkRigidbody>
    {
        [Test]
        public void InitsyncVelocityTest()
        {
            Assert.That(component.syncVelocity, Is.True);
        }

        [Test]
        public void InitvelocitySensitivityTest()
        {
            Assert.That(component.velocitySensitivity, Is.EqualTo(0.1f));
        }

        [Test]
        public void InitsyncAngularVelocityTest()
        {
            Assert.That(component.syncAngularVelocity, Is.True);
        }

        [Test]
        public void InitangularVelocitySensitivityTest()
        {
            Assert.That(component.angularVelocitySensitivity, Is.EqualTo(0.1f));
        }
    }
}
