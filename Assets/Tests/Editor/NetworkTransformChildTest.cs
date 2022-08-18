using NUnit.Framework;

namespace Mirage.Tests
{
    [TestFixture]
    public class NetworkTransformChildTest : TestBase
    {
        [Test]
        public void TargetComponentTest()
        {
            var networkTransformChild = CreateBehaviour<NetworkTransformChild>();

            Assert.That(networkTransformChild.Target == null);

            networkTransformChild.Target = networkTransformChild.transform;

            Assert.That(networkTransformChild.Target == networkTransformChild.transform);
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }
    }
}
