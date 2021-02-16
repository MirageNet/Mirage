using Mirage;

namespace ServerRpcTests.ServerRpcThatIgnoresAuthority
{
    class ServerRpcThatIgnoresAuthority : NetworkBehaviour
    {
        [ServerRpc(requireAuthority = false)]
        void CmdFunction()
        {
            // do something
        }
    }
}
