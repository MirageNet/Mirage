using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourServer
{
    class MonoBehaviourServer : MonoBehaviour
    {
        [Server]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
