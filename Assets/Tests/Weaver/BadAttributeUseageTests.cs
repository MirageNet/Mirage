using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class BadAttributeUseageTests : WeaverTestBase
    {
        [Test, BatchSafe]
        public void MonoBehaviourValid()
        {
            IsSuccess();
        }

        [Test]
        public void MonoBehaviourSyncVar()
        {
            HasErrorCount(1);
            HasError("SyncVar potato must be inside a NetworkBehaviour. MonoBehaviourSyncVar is not a NetworkBehaviour",
              "System.Int32 BadAttributeUseageTests.MonoBehaviourSyncVar.MonoBehaviourSyncVar::potato");
        }

        [Test]
        public void MonoBehaviourSyncList()
        {
            HasErrorCount(1);
            HasError("potato is a SyncObject and can not be used inside Monobehaviour. MonoBehaviourSyncList is not a NetworkBehaviour",
              "Mirage.Collections.SyncList`1<System.Int32> BadAttributeUseageTests.MonoBehaviourSyncList.MonoBehaviourSyncList::potato");
        }

        [Test]
        public void MonoBehaviourServerRpc()
        {
            HasErrorCount(1);
            HasError("ServerRpcAttribute method CmdThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourServerRpc.MonoBehaviourServerRpc::CmdThisCantBeOutsideNetworkBehaviour()");
        }

        [Test]
        public void MonoBehaviourClientRpc()
        {
            HasErrorCount(1);
            HasError("ClientRpcAttribute method RpcThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourClientRpc.MonoBehaviourClientRpc::RpcThisCantBeOutsideNetworkBehaviour()");
        }

        [Test]
        public void MonoBehaviourServer()
        {
            HasErrorCount(1);
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourServer.MonoBehaviourServer::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test]
        public void MonoBehaviourServerCallback()
        {
            HasErrorCount(1);
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourServerCallback.MonoBehaviourServerCallback::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test]
        public void MonoBehaviourClient()
        {
            HasErrorCount(1);
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourClient.MonoBehaviourClient::ThisCantBeOutsideNetworkBehaviour()");
        }

        [Test]
        public void MonoBehaviourClientCallback()
        {
            HasErrorCount(1);
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.MonoBehaviourClientCallback.MonoBehaviourClientCallback::ThisCantBeOutsideNetworkBehaviour()");
        }


        [Test]
        public void NormalClassClient()
        {
            HasErrorCount(1);
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassClient.NormalClassClient::ThisCantBeOutsideNetworkBehaviour()");
        }
        [Test]
        public void NormalClassClientCallback()
        {
            HasErrorCount(1);
            HasError("ClientAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassClientCallback.NormalClassClientCallback::ThisCantBeOutsideNetworkBehaviour()");
        }
        [Test]
        public void NormalClassClientRpc()
        {
            HasErrorCount(1);
            HasError("ClientRpcAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassClientRpc.NormalClassClientRpc::ThisCantBeOutsideNetworkBehaviour()");
        }
        [Test]
        public void NormalClassServer()
        {
            HasErrorCount(1);
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassServer.NormalClassServer::ThisCantBeOutsideNetworkBehaviour()");
        }
        [Test]
        public void NormalClassServerCallback()
        {
            HasErrorCount(1);
            HasError("ServerAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassServerCallback.NormalClassServerCallback::ThisCantBeOutsideNetworkBehaviour()");
        }
        [Test]
        public void NormalClassServerRpc()
        {
            HasErrorCount(1);
            HasError("ServerRpcAttribute method ThisCantBeOutsideNetworkBehaviour must be declared in a NetworkBehaviour",
              "System.Void BadAttributeUseageTests.NormalClassServerRpc.NormalClassServerRpc::ThisCantBeOutsideNetworkBehaviour()");
        }
        [Test]
        public void NormalClassSyncVar()
        {
            HasErrorCount(1);
            HasError("SyncVar potato must be inside a NetworkBehaviour. NormalClassSyncVar is not a NetworkBehaviour",
              "System.Int32 BadAttributeUseageTests.NormalClassSyncVar.NormalClassSyncVar::potato");
        }
    }
}
