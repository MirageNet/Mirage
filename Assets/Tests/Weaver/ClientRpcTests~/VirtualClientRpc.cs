using Mirage;

namespace ClientRpcTests.VirtualClientRpc
{
    class VirtualCommand : NetworkBehaviour
    {
        [ClientRpc]
        protected virtual void RpcDoSomething()
        {
            // do something
        }
    }
}
