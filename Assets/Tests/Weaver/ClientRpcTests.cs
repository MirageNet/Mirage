using NUnit.Framework;

namespace Mirage.Weaver
{
    public class ClientRpcTests : TestsBuildFromTestName
    {
        [Test]
        public void ClientRpcValid()
        {
            IsSuccess();
        }

        [Test]
        public void ClientRpcCantBeStatic()
        {
            HasError("RpcCantBeStatic must not be static",
                "System.Void ClientRpcTests.ClientRpcCantBeStatic.ClientRpcCantBeStatic::RpcCantBeStatic()");
        }

        [Test]
        public void VirtualClientRpc()
        {
            IsSuccess();
        }

        [Test]
        public void OverrideVirtualClientRpc()
        {
            IsSuccess();
        }

        [Test]
        public void AbstractClientRpc()
        {
            HasError("Abstract Rpcs are currently not supported, use virtual method instead",
                "System.Void ClientRpcTests.AbstractClientRpc.AbstractClientRpc::RpcDoSomething()");
        }

        [Test]
        public void OverrideAbstractClientRpc()
        {
            HasError("Abstract Rpcs are currently not supported, use virtual method instead",
                "System.Void ClientRpcTests.OverrideAbstractClientRpc.BaseBehaviour::RpcDoSomething()");
        }

        [Test]
        public void ClientRpcThatExcludesOwner()
        {
            IsSuccess();
        }

        [Test]
        public void ClientRpcConnCantSkipNetworkConn()
        {
            HasError("ClientRpc with Client.Connection needs a network connection parameter", "System.Void ClientRpcTests.ClientRpcConnCantSkipNetworkConn.ClientRpcConnCantSkipNetworkConn::ClientRpcMethod()");
        }

        [Test]
        public void ClientRpcOwnerCantExcludeOwner()
        {
            HasError("ClientRpc with Client.Owner cannot have excludeOwner set as true", "System.Void ClientRpcTests.ClientRpcOwnerCantExcludeOwner.ClientRpcOwnerCantExcludeOwner::ClientRpcMethod()");
        }
    }
}
