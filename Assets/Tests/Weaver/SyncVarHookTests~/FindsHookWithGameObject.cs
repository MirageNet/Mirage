using Mirage;
using UnityEngine;

namespace SyncVarHookTests.FindsHookWithGameObject
{
    class FindsHookWithGameObject : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onTargetChanged))]
        GameObject target { get; set; }

        void onTargetChanged(GameObject oldValue, GameObject newValue)
        {

        }
    }
}
