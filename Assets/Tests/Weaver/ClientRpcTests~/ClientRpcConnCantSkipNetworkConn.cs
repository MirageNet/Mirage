using Mirage;

namespace ClientRpcTests.ClientRpcConnCantSkipNetworkConn
{
    class ClientRpcConnCantSkipNetworkConn : NetworkBehaviour
    {
        [ClientRpc(target = RpcTarget.Player)]
        void ClientRpcMethod() { }
    }
}
