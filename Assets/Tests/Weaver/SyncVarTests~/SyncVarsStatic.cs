using Mirage;

namespace SyncVarTests.SyncVarsStatic
{
    class SyncVarsStatic : NetworkBehaviour
    {
        [SyncVar]
        static int invalidVar { get; set; } = 123;
    }
}
