using Mirage;
using UnityEngine;

namespace SyncListTests.SyncListNestedInStructWithInvalid
{
    class SyncListNestedInStructWithInvalid : NetworkBehaviour
    {
        SomeData.SyncList Foo;


        public struct SomeData
        {
            public int usefulNumber;
            public Object target;

            public class SyncList : Mirage.SyncList<SomeData> { }
        }
    }
}
