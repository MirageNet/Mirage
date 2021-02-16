using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcParamRef
{
    class NetworkBehaviourClientRpcParamRef : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcCantHaveParamRef(ref int monkeys) { }
    }
}
