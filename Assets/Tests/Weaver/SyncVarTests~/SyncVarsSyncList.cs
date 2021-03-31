using Mirage;
using Mirage.Collections;

namespace SyncVarTests.SyncVarsSyncList
{

    class SyncVarsSyncList : NetworkBehaviour
    {
        [SyncVar]
        SyncList<int> syncints;
    }
}
