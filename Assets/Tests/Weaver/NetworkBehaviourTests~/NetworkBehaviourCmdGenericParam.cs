using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourCmdGenericParam
{
    class NetworkBehaviourCmdGenericParam : NetworkBehaviour
    {
        [ServerRpc]
        public void CmdCantHaveGeneric<T>() { }
    }
}
