using Mirage;

namespace ClientRpcTests.ClientRpcValid
{
    class ClientRpcValid : NetworkBehaviour
    {
        [ClientRpc]
        void RpcThatIsTotallyValid() { }
    }
}
