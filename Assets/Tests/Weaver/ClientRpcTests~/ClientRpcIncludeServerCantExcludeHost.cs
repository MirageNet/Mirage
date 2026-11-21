using Mirage;

namespace ClientRpcTests.ClientRpcIncludeServerCantExcludeHost
{
    class ClientRpcIncludeServerCantExcludeHost : NetworkBehaviour
    {
        [ClientRpc(includeServer = true, excludeHost = true)]
        void ClientRpcMethod() { }
    }
}
