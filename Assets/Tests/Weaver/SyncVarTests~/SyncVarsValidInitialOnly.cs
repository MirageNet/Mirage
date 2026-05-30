using Mirage;

namespace SyncVarTests.SyncVarsValidInitialOnly
{
    class SyncVarsValidInitialOnly : NetworkBehaviour
    {
        [SyncVar(initialOnly = true)] int var { get; set; }
        [SyncVar(initialOnly = false)] int var2 { get; set; }
    }
}
