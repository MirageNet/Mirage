using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourServerRpc
{
    class MonoBehaviourServerRpc : MonoBehaviour
    {
        [ServerRpc]
        void CmdThisCantBeOutsideNetworkBehaviour() { }
    }
}
