using Mirage;
using UnityEngine;

namespace MonoBehaviourTests.MonoBehaviourClientCallback
{
    class MonoBehaviourClientCallback : MonoBehaviour
    {
        [Client(error = false)]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
