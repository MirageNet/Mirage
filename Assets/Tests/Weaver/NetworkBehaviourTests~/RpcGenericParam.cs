using Mirage;

namespace NetworkBehaviourTests.RpcGenericParam
{
    class RpcGenericParam : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcCantHaveGeneric<T>() { }
    }
}
