using Mirage;

namespace ClientRpcTests.ClientRpcThatExcludesOwner
{
    class ClientRpcThatExcludesOwner : NetworkBehaviour
    {
        [ClientRpc(excludeOwner = true)]
        void RpcDoSomething()
        {
            // do something
        }
    }
}
