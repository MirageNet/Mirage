using NUnit.Framework;

namespace Mirror.Weaver.Tests
{
    public class WeaverTargetRpcTests : WeaverTestsBuildFromTestName
    {
        [Test]
        public void TargetRpcCanSkipNetworkConnection()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void TargetRpcCanHaveOtherParametersWhileSkipingNetworkConnection()
        {
            Assert.That(weaverErrors, Is.Empty);
        }

        [Test]
        public void ErrorWhenNetworkConnectionIsNotTheFirstParameter()
        {
            Assert.That(weaverErrors, Contains.Item("TargetRpcMethod has invalid parameter nc, Cannot pass NetworkConnections (at System.Void WeaverTargetRpcTests.ErrorWhenNetworkConnectionIsNotTheFirstParameter.ErrorWhenNetworkConnectionIsNotTheFirstParameter::TargetRpcMethod(System.Int32,Mirror.NetworkConnection))"));
        }
    }
}
