using Mirage;
using Mirage.Collections;

namespace SyncSetTests.SyncSetErrorWhenNotInitialized
{
    class SyncSetBehaviour : NetworkBehaviour
    {
        public SyncHashSet<int> Foo;
    }
}
