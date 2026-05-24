using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListNestedStruct
{
    class SyncListNestedStruct : NetworkBehaviour
    {
        MyNestedStructList Foo = new MyNestedStructList();

        struct MyNestedStruct
        {
            public int potato;
            public float floatingpotato;
            public double givemetwopotatoes;
        }
        class MyNestedStructList : SyncList<MyNestedStruct>
        {
            public MyNestedStructList() : base(10) {}
        }
    }
}
