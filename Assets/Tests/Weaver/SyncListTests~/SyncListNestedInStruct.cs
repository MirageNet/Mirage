using Mirage;

namespace SyncListTests.SyncListNestedInStruct
{
    class SyncListNestedStruct : NetworkBehaviour
    {
        SomeData.SyncList Foo;


        public struct SomeData
        {
            public int usefulNumber;

            public class SyncList : Mirage.SyncList<SomeData> { }
        }
    }
}
