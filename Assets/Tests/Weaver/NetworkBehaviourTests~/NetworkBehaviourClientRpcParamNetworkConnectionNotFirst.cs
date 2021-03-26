using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcParamNetworkConnectionNotFirst
{
    class NetworkBehaviourClientRpcParamNetworkConnectionNotFirst : NetworkBehaviour
    {
        [ClientRpc(target = Mirage.Client.Player)]
        public void ClientRpcCantHaveParamOptional(int abc, NetworkPlayer monkeyCon) { }
    }
}
