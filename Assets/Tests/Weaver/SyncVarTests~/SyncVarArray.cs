using Mirage;

namespace SyncVarTests.SyncVarArray
{
    class SyncVarArray : NetworkBehaviour
    {
        [SyncVar]
        int[] intArray;

        [SyncVar(initialOnly = true)]
        byte[] byteArray;
    }
}
