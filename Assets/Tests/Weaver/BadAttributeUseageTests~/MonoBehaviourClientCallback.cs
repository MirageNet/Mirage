using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourClientCallback
{
    class MonoBehaviourClientCallback : MonoBehaviour
    {
        [Client(error = false)]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
