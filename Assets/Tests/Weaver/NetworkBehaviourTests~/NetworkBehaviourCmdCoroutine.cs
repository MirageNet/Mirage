using System.Collections;
using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourCmdCoroutine
{
    class NetworkBehaviourCmdCoroutine : NetworkBehaviour
    {
        [ServerRpc]
        public IEnumerator CmdCantHaveCoroutine()
        {
            yield return null;
        }
    }
}
