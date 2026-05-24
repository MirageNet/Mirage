using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListStruct
{
    class SyncListStruct : NetworkBehaviour
    {
        MyStructList Foo = new MyStructList();

        struct MyStruct
        {
            public int potato;
            public float floatingpotato;
            public double givemetwopotatoes;
        }
        class MyStructList : SyncList<MyStruct> { public MyStructList() : base(10) {} }
    }
}
