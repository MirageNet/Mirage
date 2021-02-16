using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourCmdVoidReturn
{
    class NetworkBehaviourCmdVoidReturn : NetworkBehaviour
    {
        [ServerRpc]
        public int CmdCantHaveNonVoidReturn()
        {
            return 1;
        }
    }
}
