using Mirage;

namespace ClientRpcTests.ClientRpcIncludeServerCantUseOwner
{
    class ClientRpcIncludeServerCantUseOwner : NetworkBehaviour
    {
        [ClientRpc(target = RpcTarget.Owner, includeServer = true)]
        void ClientRpcMethod() { }
    }
}
