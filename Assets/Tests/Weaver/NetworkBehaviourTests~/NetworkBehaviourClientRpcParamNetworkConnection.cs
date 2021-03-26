using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcParamNetworkConnection
{
    class NetworkBehaviourClientRpcParamNetworkConnection : NetworkBehaviour
    {
        [ClientRpc(target = Mirage.Client.Player)]
        public void RpcCantHaveParamOptional(NetworkPlayer monkeyCon) { }
    }
}
