using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryStructItem
{
    class SyncDictionaryStructItem : NetworkBehaviour
    {
        MyStructDictionary Foo = new MyStructDictionary();

        struct MyStruct
        {
            public int potato;
            public float floatingpotato;
            public double givemetwopotatoes;
        }
        class MyStructDictionary : SyncDictionary<int, MyStruct> { public MyStructDictionary() : base(10) {} }
    }
}
