using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourTargetRpc
{
    class MonoBehaviourTargetRpc : MonoBehaviour
    {
        [TargetRpc]
        void TargetThisCantBeOutsideNetworkBehaviour(NetworkConnection nc) { }
    }
}
