using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class ServerRpcTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void ServerRpcValid()
        {
            IsSuccess();
        }

        [Test]
        public void ServerRpcCantBeStatic()
        {
            HasError("CmdCantBeStatic must not be static", "System.Void ServerRpcTests.ServerRpcCantBeStatic.ServerRpcCantBeStatic::CmdCantBeStatic()");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ServerRpcThatIgnoresAuthority()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ServerRpcWithArguments()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ServerRpcThatIgnoresAuthorityWithSenderConnection()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ServerRpcWithSenderConnectionAndOtherArgs()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ServerRpcWithSenderConnectionAndOtherArgsWrongOrder()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void VirtualServerRpc()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void OverrideVirtualServerRpc()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void OverrideVirtualCallBaseServerRpc()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void OverrideVirtualCallsBaseServerRpcWithMultipleBaseClasses()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
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

        [Test, BatchSafe(BatchType.Success)]
        public void ServerRpcWithReturn()
        {
            IsSuccess();
        }

        [Test]
        public void RateLimitNegativeInterval()
        {
            HasError("[RateLimit] Interval must be greater than 0", "System.Void ServerRpcTests.RateLimitNegativeInterval.RateLimitNegativeInterval::DoSomething()");
        }

        [Test]
        public void RateLimitZeroRefill()
        {
            HasError("[RateLimit] Refill must be greater than 0", "System.Void ServerRpcTests.RateLimitZeroRefill.RateLimitZeroRefill::DoSomething()");
        }

        [Test]
        public void RateLimitZeroMaxTokens()
        {
            HasError("[RateLimit] MaxTokens must be greater than 0", "System.Void ServerRpcTests.RateLimitZeroMaxTokens.RateLimitZeroMaxTokens::DoSomething()");
        }

        [Test]
        public void RateLimitNegativePenalty()
        {
            HasError("[RateLimit] Penalty must be greater than or equal to 0", "System.Void ServerRpcTests.RateLimitNegativePenalty.RateLimitNegativePenalty::DoSomething()");
        }
    }
}
