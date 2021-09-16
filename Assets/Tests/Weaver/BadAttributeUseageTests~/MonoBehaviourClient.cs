using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourClient
{
    class MonoBehaviourClient : MonoBehaviour
    {
        [Client]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
