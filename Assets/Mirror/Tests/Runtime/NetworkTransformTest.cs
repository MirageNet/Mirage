using Mirror.Experimental;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class NetworkTransformTest : HostSetup<NetworkTransformBase>
    {
        [Test]
        public void InitexcludeOwnerUpdateTest()
        {
            Assert.That(component.excludeOwnerUpdate, Is.True);
        }

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
        public void InitinterpolatePositionTest()
        {
            Assert.That(component.interpolatePosition, Is.True);
        }

        [Test]
        public void InitinterpolateRotationTest()
        {
            Assert.That(component.interpolateRotation, Is.True);
        }

        [Test]
        public void InitinterpolateScaleTest()
        {
            Assert.That(component.interpolateScale, Is.True);
        }

        [Test]
        public void InitlocalPositionSensitivityTest()
        {
            Assert.That(component.localPositionSensitivity, Is.EqualTo(0.1f));
        }

        [Test]
        public void InitlocalRotationSensitivityTest()
        {
            Assert.That(component.localRotationSensitivity, Is.EqualTo(0.1f));
        }

        [Test]
        public void InitlocalScaleSensitivityTest()
        {
            Assert.That(component.localScaleSensitivity, Is.EqualTo(0.1f));
        }
    }
}
