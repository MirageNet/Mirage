using Mirage;
using UnityEngine;

namespace MonoBehaviourTests.MonoBehaviourTargetRpc
{
    class MonoBehaviourTargetRpc : MonoBehaviour
    {
        [TargetRpc]
        void TargetThisCantBeOutsideNetworkBehaviour(NetworkConnection nc) { }
    }
}
