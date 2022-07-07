using UnityEngine;
using Mirage;

namespace BadAttributeUseageTests.MonoBehaviourValid
{
    class MonoBehaviourValid : MonoBehaviour
    {
        int monkeys = 12;
    }

    // we must reference mirage so  that weaver will run
    class GoodBehaviour: NetworkBehaviour {}
}
