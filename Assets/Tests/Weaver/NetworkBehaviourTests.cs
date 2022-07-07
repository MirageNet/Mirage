using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class NetworkBehaviourTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourValid()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourAbstractBaseValid()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourGeneric()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourGenericInherit()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourCmdGenericArgument()
        {
            IsSuccess();
        }

        [Test]
        public void NetworkBehaviourCmdGenericParam()
        {
            HasError("CmdCantHaveGeneric cannot have generic parameters",
                "System.Void NetworkBehaviourTests.NetworkBehaviourCmdGenericParam.NetworkBehaviourCmdGenericParam::CmdCantHaveGeneric()");
        }

        [Test]
        public void NetworkBehaviourCmdCoroutine()
        {
            HasError("CmdCantHaveCoroutine cannot be a coroutine",
                "System.Collections.IEnumerator NetworkBehaviourTests.NetworkBehaviourCmdCoroutine.NetworkBehaviourCmdCoroutine::CmdCantHaveCoroutine()");
        }

        [Test]
        public void NetworkBehaviourCmdVoidReturn()
        {
            HasError("Use UniTask<System.Int32> to return values from [ServerRpc]",
                "System.Int32 NetworkBehaviourTests.NetworkBehaviourCmdVoidReturn.NetworkBehaviourCmdVoidReturn::CmdCantHaveNonVoidReturn()");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourClientRpcGenericArgument()
        {
            IsSuccess();
        }

        [Test]
        public void NetworkBehaviourClientRpcGenericParam()
        {
            HasError("RpcCantHaveGeneric cannot have generic parameters",
                "System.Void NetworkBehaviourTests.NetworkBehaviourClientRpcGenericParam.NetworkBehaviourClientRpcGenericParam::RpcCantHaveGeneric()");
        }

        [Test]
        public void NetworkBehaviourClientRpcCoroutine()
        {
            HasError("RpcCantHaveCoroutine cannot be a coroutine",
                "System.Collections.IEnumerator NetworkBehaviourTests.NetworkBehaviourClientRpcCoroutine.NetworkBehaviourClientRpcCoroutine::RpcCantHaveCoroutine()");
        }

        [Test]
        public void NetworkBehaviourClientRpcVoidReturn()
        {
            HasError("[ClientRpc] must return void",
                "System.Int32 NetworkBehaviourTests.NetworkBehaviourClientRpcVoidReturn.NetworkBehaviourClientRpcVoidReturn::RpcCantHaveNonVoidReturn()");
        }

        [Test]
        public void NetworkBehaviourClientRpcParamOut()
        {
            HasError("RpcCantHaveParamOut cannot have out parameters",
                "System.Void NetworkBehaviourTests.NetworkBehaviourClientRpcParamOut.NetworkBehaviourClientRpcParamOut::RpcCantHaveParamOut(System.Int32&)");
        }

        [Test]
        public void NetworkBehaviourClientRpcParamOptional()
        {
            HasError("RpcCantHaveParamOptional cannot have optional parameters",
                "System.Void NetworkBehaviourTests.NetworkBehaviourClientRpcParamOptional.NetworkBehaviourClientRpcParamOptional::RpcCantHaveParamOptional(System.Int32)");
        }

        [Test]
        public void NetworkBehaviourClientRpcParamRef()
        {
            HasError("Cannot pass Int32& by reference",
                "System.Int32&");
            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void NetworkBehaviourTests.NetworkBehaviourClientRpcParamRef.NetworkBehaviourClientRpcParamRef::RpcCantHaveParamRef(System.Int32&)");
        }

        [Test]
        public void NetworkBehaviourClientRpcParamAbstract()
        {
            HasError("Cannot generate write function for abstract class AbstractClass. Use a supported type or provide a custom write function",
                "NetworkBehaviourTests.NetworkBehaviourClientRpcParamAbstract.NetworkBehaviourClientRpcParamAbstract/AbstractClass");
            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void NetworkBehaviourTests.NetworkBehaviourClientRpcParamAbstract.NetworkBehaviourClientRpcParamAbstract::RpcCantHaveParamAbstract(NetworkBehaviourTests.NetworkBehaviourClientRpcParamAbstract.NetworkBehaviourClientRpcParamAbstract/AbstractClass)");
        }

        [Test]
        public void NetworkBehaviourClientRpcParamComponent()
        {
            HasError("Cannot generate write function for component type ComponentClass. Use a supported type or provide a custom write function",
                "NetworkBehaviourTests.NetworkBehaviourClientRpcParamComponent.NetworkBehaviourClientRpcParamComponent/ComponentClass");
            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void NetworkBehaviourTests.NetworkBehaviourClientRpcParamComponent.NetworkBehaviourClientRpcParamComponent::RpcCantHaveParamComponent(NetworkBehaviourTests.NetworkBehaviourClientRpcParamComponent.NetworkBehaviourClientRpcParamComponent/ComponentClass)");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourClientRpcParamNetworkConnection()
        {
            IsSuccess();
        }

        [Test]
        public void NetworkBehaviourClientRpcParamNetworkConnectionNotFirst()
        {
            HasError("ClientRpcCantHaveParamOptional has invalid parameter monkeyCon, Cannot pass NetworkConnections", "System.Void NetworkBehaviourTests.NetworkBehaviourClientRpcParamNetworkConnectionNotFirst.NetworkBehaviourClientRpcParamNetworkConnectionNotFirst::ClientRpcCantHaveParamOptional(System.Int32,Mirage.INetworkPlayer)");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourClientRpcDuplicateName()
        {
            IsSuccess();
        }

        [Test]
        public void NetworkBehaviourCmdParamOut()
        {
            HasError("CmdCantHaveParamOut cannot have out parameters",
                "System.Void NetworkBehaviourTests.NetworkBehaviourCmdParamOut.NetworkBehaviourCmdParamOut::CmdCantHaveParamOut(System.Int32&)");
        }

        [Test]
        public void NetworkBehaviourCmdParamOptional()
        {
            HasError("CmdCantHaveParamOptional cannot have optional parameters",
                "System.Void NetworkBehaviourTests.NetworkBehaviourCmdParamOptional.NetworkBehaviourCmdParamOptional::CmdCantHaveParamOptional(System.Int32)");
        }

        [Test]
        public void NetworkBehaviourCmdParamRef()
        {
            HasError("Cannot pass Int32& by reference", "System.Int32&");
            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void NetworkBehaviourTests.NetworkBehaviourCmdParamRef.NetworkBehaviourCmdParamRef::CmdCantHaveParamRef(System.Int32&)");
        }

        [Test]
        public void NetworkBehaviourCmdParamAbstract()
        {
            HasError("Cannot generate write function for abstract class AbstractClass. Use a supported type or provide a custom write function",
                "NetworkBehaviourTests.NetworkBehaviourCmdParamAbstract.NetworkBehaviourCmdParamAbstract/AbstractClass");
            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void NetworkBehaviourTests.NetworkBehaviourCmdParamAbstract.NetworkBehaviourCmdParamAbstract::CmdCantHaveParamAbstract(NetworkBehaviourTests.NetworkBehaviourCmdParamAbstract.NetworkBehaviourCmdParamAbstract/AbstractClass)");
        }

        [Test]
        public void NetworkBehaviourCmdParamComponent()
        {
            HasError("Cannot generate write function for component type ComponentClass. Use a supported type or provide a custom write function",
                "NetworkBehaviourTests.NetworkBehaviourCmdParamComponent.NetworkBehaviourCmdParamComponent/ComponentClass");
            HasError("Could not process Rpc because one or more of its parameter were invalid",
                "System.Void NetworkBehaviourTests.NetworkBehaviourCmdParamComponent.NetworkBehaviourCmdParamComponent::CmdCantHaveParamComponent(NetworkBehaviourTests.NetworkBehaviourCmdParamComponent.NetworkBehaviourCmdParamComponent/ComponentClass)");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourCmdParamGameObject()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourCmdDuplicateName()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourChild()
        {
            IsSuccess();
        }
    }
}
