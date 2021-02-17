using Mirage;

namespace ClientRpcTests.ClientRpcOwnerCantExcludeOwner
{
    class ClientRpcOwnerCantExcludeOwner : NetworkBehaviour
    {
        [ClientRpc(target = Mirage.Client.Owner, excludeOwner = true)]
        void ClientRpcMethod() { }
    }
}
