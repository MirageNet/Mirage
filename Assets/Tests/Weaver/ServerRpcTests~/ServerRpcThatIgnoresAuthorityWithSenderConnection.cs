using Mirage;

namespace ServerRpcTests.ServerRpcThatIgnoresAuthorityWithSenderConnection
{
    class ServerRpcThatIgnoresAuthorityWithSenderConnection : NetworkBehaviour
    {
        [ServerRpc(requireAuthority = false)]
        void CmdFunction(NetworkPlayer connection = null)
        {
            // do something
        }
    }
}
