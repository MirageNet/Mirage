using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryInheritance
{
    class SyncDictionaryInheritance : NetworkBehaviour
    {
        readonly SuperDictionary dictionary = new SuperDictionary();


        public class SomeDictionary<TKey, TItem> : SyncDictionary<TKey, TItem> { public SomeDictionary() : base(10) {} }

        public class SomeDictionaryIntString : SomeDictionary<int, string> { public SomeDictionaryIntString() : base() {} }

        public class SuperDictionary : SomeDictionaryIntString
        {
            public SuperDictionary() : base() {}
        }
    }
}
