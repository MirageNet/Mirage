using Mirage;

namespace ServerRpcTests.ServerRpcWithSenderConnectionAndOtherArgsWrongOrder
{
    class ServerRpcWithSenderConnectionAndOtherArgsWrongOrder : NetworkBehaviour
    {
        [ServerRpc(requireAuthority = false)]
        void CmdFunction(int someNumber, INetworkPlayer connection, NetworkIdentity someTarget)
        {
            // do something
        }
    }
}