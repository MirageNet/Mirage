using Mirage;
using UnityEngine;
using Mirage.Collections;

namespace SyncListTests.SyncListNestedInStructWithInvalid
{
    class SyncListNestedInStructWithInvalid : NetworkBehaviour
    {
        SomeData.SyncList Foo = new SomeData.SyncList();


        public struct SomeData
        {
            public int usefulNumber;
            public Object target;

            public class SyncList : Mirage.Collections.SyncList<SomeData> { public SyncList() : base(10) {} }
        }
    }
}
