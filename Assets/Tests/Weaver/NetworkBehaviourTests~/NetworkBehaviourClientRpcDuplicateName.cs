using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcDuplicateName
{
    class NetworkBehaviourClientRpcDuplicateName : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcCantHaveSameName(int abc) { }

        [ClientRpc]
        public void RpcCantHaveSameName(int abc, int def) { }
    }
}
