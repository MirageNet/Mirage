using Mirage;

namespace ServerRpcTests.ServerRpcWithSenderConnectionAndOtherArgs
{
    class ServerRpcWithSenderConnectionAndOtherArgs : NetworkBehaviour
    {
        [ServerRpc(requireAuthority = false)]
        void CmdFunction(int someNumber, INetworkPlayer connection, NetworkIdentity someTarget)
        {
            // do something
        }
    }
}