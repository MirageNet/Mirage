using Mirage;
using UnityEngine;

namespace MonoBehaviourTests.MonoBehaviourClientRpc
{
    class MonoBehaviourClientRpc : MonoBehaviour
    {
        [ClientRpc]
        void RpcThisCantBeOutsideNetworkBehaviour() { }
    }
}
