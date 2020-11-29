using Mirror.Tests;
using NUnit.Framework;

namespace Mirror.Experimental.Tests
{
    [TestFixture]
    public class NetworkTransformStateTest : HostSetup<NetworkTransformState>
    {
        [Test]
        public void InitsyncPositionTest()
        {
            Assert.That(component.syncPosition, Is.True);
        }

        [Test]
        public void InitsyncRotationTest()
        {
            Assert.That(component.syncRotation, Is.True);
        }

        [Test]
        public void InitsyncScaleTest()
        {
            Assert.That(component.syncScale, Is.True);
        }

        [Test]
        public void InitlocalPositionSensitivityTest()
        {
            Assert.That(component.localPositionSensitivity, Is.InRange(0.001f, 0.199f));
        }

        [Test]
        public void InitlocalRotationSensitivityTest()
        {
            Assert.That(component.localRotationSensitivity, Is.InRange(0.001f, 0.199f));
        }

        [Test]
        public void InitlocalScaleSensitivityTest()
        {
            Assert.That(component.localScaleSensitivity, Is.InRange(0.001f, 0.199f));
        }
    }
}
