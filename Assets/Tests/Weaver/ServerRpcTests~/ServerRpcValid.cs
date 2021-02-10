using Mirage;

namespace ServerRpcTests.ServerRpcValid
{
    class ServerRpcValid : NetworkBehaviour
    {
        [ServerRpc]
        void CmdThatIsTotallyValid() { }
    }
}
