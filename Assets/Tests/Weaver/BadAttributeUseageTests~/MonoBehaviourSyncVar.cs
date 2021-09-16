using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.MonoBehaviourSyncVar
{
    class MonoBehaviourSyncVar : MonoBehaviour
    {
        [SyncVar]
        int potato;
    }
}
