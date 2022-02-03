using Mirage;

namespace NetworkBehaviourTests.RpcGenericParamWithArg
{
    class RpcGenericParamWithArg : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcCantHaveGeneric<T>(T value) { }
    }
}
