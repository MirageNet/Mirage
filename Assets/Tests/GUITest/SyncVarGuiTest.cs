using UnityEngine;

namespace Mirage.Tests.GUiTests
{
    public class SyncVarGuiTest : NetworkBehaviour
    {
        [SyncVar] public int Number { get; set; }
        [SyncVar] public string Str { get; set; }
        [SyncVar] public GameObject Obj { get; set; }
        [SyncVar] public NetworkIdentity IdentityField { get; set; }
        [SyncVar] public NoSyncGuiTest BehaviovurField { get; set; }
    }

}
