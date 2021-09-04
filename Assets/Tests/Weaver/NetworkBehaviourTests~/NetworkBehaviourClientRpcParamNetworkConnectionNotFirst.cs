using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcParamNetworkConnectionNotFirst
{
    class NetworkBehaviourClientRpcParamNetworkConnectionNotFirst : NetworkBehaviour
    {
        [ClientRpc(target = RpcTarget.Player)]
        public void ClientRpcCantHaveParamOptional(int abc, INetworkPlayer monkeyCon) { }
    }
}
