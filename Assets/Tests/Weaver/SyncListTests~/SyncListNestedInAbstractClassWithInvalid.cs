using Mirage;
using UnityEngine;
using Mirage.Collections;

namespace SyncListTests.SyncListNestedInAbstractClassWithInvalid
{
    class SyncListNestedStructWithInvalid : NetworkBehaviour
    {
        SomeAbstractClass.MyNestedStructList Foo;


        public abstract class SomeAbstractClass
        {
            public struct MyNestedStruct
            {
                public int potato;
                public Object target;
            }
            public class MyNestedStructList : SyncList<MyNestedStruct> { }
        }
    }
}
