using Mirage;
using UnityEngine;

namespace MonoBehaviourTests.MonoBehaviourServer
{
    class MonoBehaviourServer : MonoBehaviour
    {
        [Server]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
