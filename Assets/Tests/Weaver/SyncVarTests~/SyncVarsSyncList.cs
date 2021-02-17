using Mirage;

namespace SyncVarTests.SyncVarsSyncList
{

    class SyncVarsSyncList : NetworkBehaviour
    {
        [SyncVar]
        SyncList<int> syncints;
    }
}
