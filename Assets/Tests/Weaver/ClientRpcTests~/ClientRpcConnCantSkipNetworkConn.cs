using Mirage;

namespace ClientRpcTests.ClientRpcConnCantSkipNetworkConn
{
    class ClientRpcConnCantSkipNetworkConn : NetworkBehaviour
    {
        [ClientRpc(target = Mirage.Client.Player)]
        void ClientRpcMethod() { }
    }
}
