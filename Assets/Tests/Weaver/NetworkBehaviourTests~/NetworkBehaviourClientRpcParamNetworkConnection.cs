using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcParamNetworkConnection
{
    class NetworkBehaviourClientRpcParamNetworkConnection : NetworkBehaviour
    {
        [ClientRpc(target = RpcTarget.Player)]
        public void RpcCantHaveParamOptional(INetworkPlayer monkeyCon) { }
    }
}
