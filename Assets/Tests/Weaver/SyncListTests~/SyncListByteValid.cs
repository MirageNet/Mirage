using Mirage;

namespace SyncListTests.SyncListByteValid
{
    class SyncListByteValid : NetworkBehaviour
    {
        class MyByteClass : SyncList<byte> { };

        MyByteClass Foo;
    }
}
