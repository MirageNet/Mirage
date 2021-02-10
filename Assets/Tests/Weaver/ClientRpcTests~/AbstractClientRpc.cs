using Mirage;

namespace ClientRpcTests.AbstractClientRpc
{
    abstract class AbstractClientRpc : NetworkBehaviour
    {
        [ClientRpc]
        protected abstract void RpcDoSomething();
    }
}
