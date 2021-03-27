using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListNestedInStruct
{
    class SyncListNestedStruct : NetworkBehaviour
    {
        SomeData.SyncList Foo;


        public struct SomeData
        {
            public int usefulNumber;

            public class SyncList : Mirage.Collections.SyncList<SomeData> { }
        }
    }
}
