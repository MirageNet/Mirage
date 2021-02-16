using Mirage;

namespace SyncSetTests.SyncSetByteValid
{
    class SyncSetByteValid : NetworkBehaviour
    {
        class MyByteClass : SyncHashSet<byte> { };

        MyByteClass Foo;
    }
}
