using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourServerCallback
{
    class MonoBehaviourServerCallback : MonoBehaviour
    {
        [Server(error = false)]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
