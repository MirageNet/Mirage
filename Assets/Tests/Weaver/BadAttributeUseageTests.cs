using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class BadAttributeUseageTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void MonoBehaviourValid()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourSyncVar()
        {
            HasError("SyncVar potato must be inside a NetworkBehaviour. MonoBehaviourSyncVar is not a NetworkBehaviour",
              "System.Int32 BadAttributeUseageTests.MonoBehaviourSyncVar.MonoBehaviourSyncVar::potato");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourSyncList()
        {
            HasError("potato is a SyncObject and can not be used inside Monobehaviour. MonoBehaviourSyncList is not a NetworkBehaviour",
              "Mirage.Collections.SyncList`1<System.Int32> BadAttributeUseageTests.MonoBehaviourSyncList.MonoBehaviourSyncList::potato");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourServerRpc()
        {
            HasError("ServerRpcAttribute method CmdThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourServerRpc.MonoBehaviourServerRpc::CmdThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourClientRpc()
        {
            HasError("ClientRpcAttribute method RpcThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourClientRpc.MonoBehaviourClientRpc::RpcThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourServer()
        {
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourServer.MonoBehaviourServer::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourServerCallback()
        {
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourServerCallback.MonoBehaviourServerCallback::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourClient()
        {
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourClient.MonoBehaviourClient::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void MonoBehaviourClientCallback()
        {
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourClientCallback.MonoBehaviourClientCallback::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void NormalClassClient()
        {
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassClient.NormalClassClient::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void NormalClassClientCallback()
        {
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassClientCallback.NormalClassClientCallback::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void NormalClassClientRpc()
        {
            HasError("ClientRpcAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassClientRpc.NormalClassClientRpc::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void NormalClassServer()
        {
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassServer.NormalClassServer::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void NormalClassServerCallback()
        {
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassServerCallback.NormalClassServerCallback::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void NormalClassServerRpc()
        {
            HasError("ServerRpcAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassServerRpc.NormalClassServerRpc::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test, BatchSafe(BatchType.Fail)]
        public void NormalClassSyncVar()
        {
            HasError("SyncVar potato must be inside a NetworkBehaviour. NormalClassSyncVar is not a NetworkBehaviour",
              "System.Int32 BadAttributeUseageTests.NormalClassSyncVar.NormalClassSyncVar::potato");
        }
    }
}
