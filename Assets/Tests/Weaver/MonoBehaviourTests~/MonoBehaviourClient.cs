using Mirage;
using UnityEngine;

namespace MonoBehaviourTests.MonoBehaviourClient
{
    class MonoBehaviourClient : MonoBehaviour
    {
        [Client]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
