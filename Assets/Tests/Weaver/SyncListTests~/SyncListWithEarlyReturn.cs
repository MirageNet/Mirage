using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListWithEarlyReturn
{
    class SyncListWithEarlyReturn : NetworkBehaviour
    {
        public SyncList<int> Foo;

        public SyncListWithEarlyReturn(bool condition)
        {
            if (condition)
                return;
        }
    }
}
