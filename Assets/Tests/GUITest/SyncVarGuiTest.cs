using UnityEngine;

namespace Mirage.Tests.GUiTests
{
    public class SyncVarGuiTest : NetworkBehaviour
    {
        [SyncVar] public int Number;
        [SyncVar] public string Str;
        [SyncVar] public GameObject Obj;
        [SyncVar] public NetworkIdentity IdentityField;
        [SyncVar] public NoSyncGuiTest BehaviovurField;
    }

}
