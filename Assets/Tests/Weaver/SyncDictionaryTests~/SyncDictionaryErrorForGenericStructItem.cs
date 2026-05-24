using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryErrorForGenericStructItem
{
    class SyncDictionaryErrorForGenericStructItem : NetworkBehaviour
    {
        struct MyGenericStruct<T>
        {
            T genericpotato;
        }

        class MyGenericStructDictionary : SyncDictionary<int, MyGenericStruct<float>> { public MyGenericStructDictionary() : base(10) {} };

        MyGenericStructDictionary harpseals = new MyGenericStructDictionary();
    }

}
