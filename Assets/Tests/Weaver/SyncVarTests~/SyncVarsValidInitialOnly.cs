using Mirage;

namespace SyncVarTests.SyncVarsValidInitialOnly
{
    class SyncVarsValidInitialOnly : NetworkBehaviour
    {
        [SyncVar(initialOnly = true)] int var;
        [SyncVar(initialOnly = false)] int var2;
    }
}
