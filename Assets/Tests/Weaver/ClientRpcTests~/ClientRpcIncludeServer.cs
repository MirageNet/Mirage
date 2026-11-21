using Mirage;

namespace ClientRpcTests.ClientRpcIncludeServer
{
    class ClientRpcIncludeServer : NetworkBehaviour
    {
        [ClientRpc(includeServer = true)]
        void RpcDoSomething() { }

        [ClientRpc(target = RpcTarget.Observers, includeServer = true)]
        void RpcDoSomethingWithPlayer(INetworkPlayer player, int value) { }
    }
}
