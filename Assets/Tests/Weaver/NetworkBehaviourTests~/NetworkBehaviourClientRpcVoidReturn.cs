using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcVoidReturn
{
    class NetworkBehaviourClientRpcVoidReturn : NetworkBehaviour
    {
        [ClientRpc]
        public int RpcCantHaveNonVoidReturn()
        {
            return 1;
        }
    }
}
