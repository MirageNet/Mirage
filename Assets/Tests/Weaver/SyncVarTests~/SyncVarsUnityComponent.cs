using Mirage;
using UnityEngine;

namespace SyncVarTests.SyncVarsUnityComponent
{
    class SyncVarsUnityComponent : NetworkBehaviour
    {
        [SyncVar]
        TextMesh invalidVar;
    }
}
