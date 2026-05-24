using Mirage;
using Mirage.Collections;

namespace SyncSetTests.SyncSetStruct
{
    class SyncSetStruct : NetworkBehaviour
    {
        MyStructSet Foo = new MyStructSet();

        struct MyStruct
        {
            public int potato;
            public float floatingpotato;
            public double givemetwopotatoes;
        }
        class MyStructSet : SyncHashSet<MyStruct>
        {
            public MyStructSet() : base(10) {}
        }
    }
}
