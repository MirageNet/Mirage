using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListByteValid
{
    class SyncListByteValid : NetworkBehaviour
    {
        class MyByteClass : SyncList<byte> { };

        MyByteClass Foo;
    }
}
