using Mirage;

namespace SyncVarTests.SyncVarsGenericParam
{
    class SyncVarsGenericParam : NetworkBehaviour
    {
        struct MySyncVar<T>
        {
            T abc;
        }

        [SyncVar]
        MySyncVar<int> invalidVar = new MySyncVar<int>();
    }
}
