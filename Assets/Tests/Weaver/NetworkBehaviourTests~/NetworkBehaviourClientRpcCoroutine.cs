using System.Collections;
using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourClientRpcCoroutine
{
    class NetworkBehaviourClientRpcCoroutine : NetworkBehaviour
    {
        [ClientRpc]
        public IEnumerator RpcCantHaveCoroutine()
        {
            yield return null;
        }
    }
}
