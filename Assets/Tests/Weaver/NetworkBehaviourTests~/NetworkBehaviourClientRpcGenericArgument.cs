using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcGenericArgument
{
    class NetworkBehaviourClientRpcGenericArgument<T> : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcCantHaveGeneric(T arg) { }
    }
}
