using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryStructKey
{
    class SyncDictionaryStructKey : NetworkBehaviour
    {
        MyStructDictionary Foo = new MyStructDictionary();

        struct MyStruct
        {
            public int potato;
            public float floatingpotato;
            public double givemetwopotatoes;
        }
        class MyStructDictionary : SyncDictionary<MyStruct, string> { public MyStructDictionary() : base(10) {} }
    }
}
