using NUnit.Framework;

namespace Mirror.Weaver.Tests
{
    public class WeaverServerRpcTests : WeaverTestsBuildFromTestName
    {
        [Test]
        public void ServerRpcValid()
        {
            IsSuccess();
        }

        [Test]
        public void ServerRpcCantBeStatic()
        {
            Assert.That(weaverLog.errors, Contains.Item("CmdCantBeStatic must not be static (at System.Void WeaverServerRpcTests.ServerRpcCantBeStatic.ServerRpcCantBeStatic::CmdCantBeStatic())"));
        }

        [Test]
        public void ServerRpcThatIgnoresAuthority()
        {
            IsSuccess();
        }

        [Test]
        public void ServerRpcWithArguments()
        {
            IsSuccess();
        }

        [Test]
        public void ServerRpcThatIgnoresAuthorityWithSenderConnection()
        {
            IsSuccess();
        }

        [Test]
        public void ServerRpcWithSenderConnectionAndOtherArgs()
        {
            IsSuccess();
        }

        [Test]
        public void VirtualServerRpc()
        {
            IsSuccess();
        }

        [Test]
        public void OverrideVirtualServerRpc()
        {
            IsSuccess();
        }

        [Test]
        public void OverrideVirtualCallBaseServerRpc()
        {
            IsSuccess();
        }

        [Test]
        public void OverrideVirtualCallsBaseServerRpcWithMultipleBaseClasses()
        {
            IsSuccess();
        }

        [Test]
        public void OverrideVirtualCallsBaseServerRpcWithOverride()
        {
            IsSuccess();
        }

        [Test]
        public void AbstractServerRpc()
        {
            Assert.That(weaverLog.errors, Contains.Item("Abstract Rpcs are currently not supported, use virtual method instead (at System.Void WeaverServerRpcTests.AbstractServerRpc.AbstractServerRpc::CmdDoSomething())"));
        }

        [Test]
        public void OverrideAbstractServerRpc()
        {
            Assert.That(weaverLog.errors, Contains.Item("Abstract Rpcs are currently not supported, use virtual method instead (at System.Void WeaverServerRpcTests.OverrideAbstractServerRpc.BaseBehaviour::CmdDoSomething())"));
        }

        [Test]
        public void ServerRpcWithReturn()
        {
            IsSuccess();
        }
    }
}
