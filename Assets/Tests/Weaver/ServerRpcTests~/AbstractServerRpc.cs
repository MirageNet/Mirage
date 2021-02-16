using Mirage;


namespace ServerRpcTests.AbstractServerRpc
{
    abstract class AbstractServerRpc : NetworkBehaviour
    {
        [ServerRpc]
        protected abstract void CmdDoSomething();
    }
}
