using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryErrorForGenericStructKey
{
    class SyncDictionaryErrorForGenericStructKey : NetworkBehaviour
    {
        MyGenericStructDictionary harpseals = new MyGenericStructDictionary();


        struct MyGenericStruct<T>
        {
            T genericpotato;
        }

        class MyGenericStructDictionary : SyncDictionary<MyGenericStruct<float>, int> { public MyGenericStructDictionary() : base(10) {} };
    }
}
