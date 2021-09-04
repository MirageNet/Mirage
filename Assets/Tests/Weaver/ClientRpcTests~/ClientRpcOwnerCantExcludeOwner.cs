using Mirage;

namespace ClientRpcTests.ClientRpcOwnerCantExcludeOwner
{
    class ClientRpcOwnerCantExcludeOwner : NetworkBehaviour
    {
        [ClientRpc(target = RpcTarget.Owner, excludeOwner = true)]
        void ClientRpcMethod() { }
    }
}
