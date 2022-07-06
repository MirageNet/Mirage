using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class ServerRpcTests : WeaverTestBase
    {
        [Test, BatchSafe]
        public void ServerRpcValid()
        {
            IsSuccess();
        }

        [Test]
        public void ServerRpcCantBeStatic()
        {
            HasError("CmdCantBeStatic must not be static", "System.Void ServerRpcTests.ServerRpcCantBeStatic.ServerRpcCantBeStatic::CmdCantBeStatic()");
        }

        [Test, BatchSafe]
        public void ServerRpcThatIgnoresAuthority()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void ServerRpcWithArguments()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void ServerRpcThatIgnoresAuthorityWithSenderConnection()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void ServerRpcWithSenderConnectionAndOtherArgs()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void ServerRpcWithSenderConnectionAndOtherArgsWrongOrder()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void VirtualServerRpc()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void OverrideVirtualServerRpc()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void OverrideVirtualCallBaseServerRpc()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void OverrideVirtualCallsBaseServerRpcWithMultipleBaseClasses()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void OverrideVirtualCallsBaseServerRpcWithOverride()
        {
            IsSuccess();
        }

        [Test]
        public void AbstractServerRpc()
        {
            HasError("Abstract Rpcs are currently not supported, use virtual method instead", "System.Void ServerRpcTests.AbstractServerRpc.AbstractServerRpc::CmdDoSomething()");
        }

        [Test]
        public void OverrideAbstractServerRpc()
        {
            HasError("Abstract Rpcs are currently not supported, use virtual method instead", "System.Void ServerRpcTests.OverrideAbstractServerRpc.BaseBehaviour::CmdDoSomething()");
        }

        [Test, BatchSafe]
        public void ServerRpcWithReturn()
        {
            IsSuccess();
        }
    }
}
