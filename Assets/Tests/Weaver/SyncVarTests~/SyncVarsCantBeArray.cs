using Mirage;

namespace SyncVarTests.SyncVarsCantBeArray
{
    class SyncVarsCantBeArray : NetworkBehaviour
    {
        [SyncVar]
        int[] thisShouldntWork { get; set; } = new int[100];
    }
}
