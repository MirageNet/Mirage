using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListErrorForGenericStruct
{
    class SyncListErrorForGenericStruct : NetworkBehaviour
    {
        MyGenericStructList harpseals = new MyGenericStructList();


        struct MyGenericStruct<T>
        {
            T genericpotato;
        }

        class MyGenericStructList : SyncList<MyGenericStruct<float>>
        {
            public MyGenericStructList() : base(10) {}
        }
    }
}
