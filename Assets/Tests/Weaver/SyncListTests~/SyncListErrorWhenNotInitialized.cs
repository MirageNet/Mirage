using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListErrorWhenNotInitialized
{
    class SyncListBehaviour : NetworkBehaviour
    {
        public SyncList<int> Foo;
    }
}
