using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourClientRpc
{
    class MonoBehaviourClientRpc : MonoBehaviour
    {
        [ClientRpc]
        void RpcThisCantBeOutsideNetworkBehaviour() { }
    }
}
