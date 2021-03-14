using Mirage;
using Mirage.Collections;

namespace SyncSetTests.SyncSetByteValid
{
    class SyncSetByteValid : NetworkBehaviour
    {
        class MyByteClass : SyncHashSet<byte> { };

        MyByteClass Foo;
    }
}
