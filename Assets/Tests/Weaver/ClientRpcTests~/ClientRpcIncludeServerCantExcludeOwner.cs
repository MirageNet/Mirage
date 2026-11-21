using Mirage;

namespace ClientRpcTests.ClientRpcIncludeServerCantExcludeOwner
{
    class ClientRpcIncludeServerCantExcludeOwner : NetworkBehaviour
    {
        [ClientRpc(includeServer = true, excludeOwner = true)]
        void ClientRpcMethod() { }
    }
}
