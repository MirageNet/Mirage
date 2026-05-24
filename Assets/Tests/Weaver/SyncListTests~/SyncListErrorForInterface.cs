using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListErrorForInterface
{
    class SyncListErrorForInterface : NetworkBehaviour
    {
        MyInterfaceList Foo = new MyInterfaceList();
    }
    interface MyInterface
    {
        int someNumber { get; set; }
    }
    class MyInterfaceList : SyncList<MyInterface>
    {
        public MyInterfaceList() : base(10) {}
    }
}
