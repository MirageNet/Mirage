using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListNestedInAbstractClass
{
    class SyncListNestedStruct : NetworkBehaviour
    {
        SomeAbstractClass.MyNestedStructList Foo = new SomeAbstractClass.MyNestedStructList();


        public abstract class SomeAbstractClass
        {
            public struct MyNestedStruct
            {
                public int potato;
                public float floatingpotato;
                public double givemetwopotatoes;
            }
            public class MyNestedStructList : SyncList<MyNestedStruct>
            {
                public MyNestedStructList() : base(10) {}
            }
        }
    }
}
