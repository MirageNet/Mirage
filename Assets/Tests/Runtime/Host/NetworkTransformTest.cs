using Mirage.Tests.Runtime.Host;
using NUnit.Framework;

namespace Mirage.Experimental.Tests.Host
{
    [TestFixture]
    public class NetworkTransformTest : HostSetup<NetworkTransform>
    {
        [Test]
        public void InitexcludeOwnerUpdateTest()
        {
            Assert.That(playerComponent.excludeOwnerUpdate, Is.True);
        }

        [Test]
        public void InitsyncPositionTest()
        {
            Assert.That(playerComponent.syncPosition, Is.True);
        }

        [Test]
        public void InitsyncRotationTest()
        {
            Assert.That(playerComponent.syncRotation, Is.True);
        }

        [Test]
        public void InitsyncScaleTest()
        {
            Assert.That(playerComponent.syncScale, Is.True);
        }

        [Test]
        public void InitinterpolatePositionTest()
        {
            Assert.That(playerComponent.interpolatePosition, Is.True);
        }

        [Test]
        public void InitinterpolateRotationTest()
        {
            Assert.That(playerComponent.interpolateRotation, Is.True);
        }

        [Test]
        public void InitinterpolateScaleTest()
        {
            Assert.That(playerComponent.interpolateScale, Is.True);
        }

        [Test]
        public void InitlocalPositionSensitivityTest()
        {
            Assert.That(playerComponent.localPositionSensitivity, Is.InRange(0.001f, 0.199f));
        }

        [Test]
        public void InitlocalRotationSensitivityTest()
        {
            Assert.That(playerComponent.localRotationSensitivity, Is.InRange(0.001f, 0.199f));
        }

        [Test]
        public void InitlocalScaleSensitivityTest()
        {
            Assert.That(playerComponent.localScaleSensitivity, Is.InRange(0.001f, 0.199f));
        }
    }
}
