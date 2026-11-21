using Mirage;

namespace ClientRpcTests.ClientRpcIncludeServerCantUsePlayer
{
    class ClientRpcIncludeServerCantUsePlayer : NetworkBehaviour
    {
        [ClientRpc(target = RpcTarget.Player, includeServer = true)]
        void ClientRpcMethod(INetworkPlayer player) { }
    }
}
