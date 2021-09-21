using Mirage;

namespace SyncVarTests.SyncVarsValidInitialOnly
{
    class SyncVarsValidInitialOnly : NetworkBehaviour
    {
        [SyncVar(InitialOnly = true)] int var;
        [SyncVar(InitialOnly = false)] int var2;
    }
}
