using Mirage;

namespace SyncVarTests.SyncVarGenericFields
{
    class SyncVarGenericFields<T> : NetworkBehaviour
    {
        [SyncVar]
        T invalidVar;
    }
}
