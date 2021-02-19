using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcParamNetworkConnection
{
    class NetworkBehaviourClientRpcParamNetworkConnection : NetworkBehaviour
    {
        [ClientRpc(target = Mirage.Client.Connection)]
        public void RpcCantHaveParamOptional(INetworkConnection monkeyCon) { }
    }
}
