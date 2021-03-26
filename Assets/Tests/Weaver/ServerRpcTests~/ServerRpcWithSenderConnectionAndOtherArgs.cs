using Mirage;

namespace ServerRpcTests.ServerRpcWithSenderConnectionAndOtherArgs
{
    class ServerRpcWithSenderConnectionAndOtherArgs : NetworkBehaviour
    {
        [ServerRpc(requireAuthority = false)]
        void CmdFunction(int someNumber, NetworkIdentity someTarget, NetworkPlayer connection = null)
        {
            // do something
        }
    }
}
