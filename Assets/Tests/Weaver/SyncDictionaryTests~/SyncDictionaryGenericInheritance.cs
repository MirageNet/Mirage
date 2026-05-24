using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryGenericInheritance
{
    class SyncDictionaryGenericInheritance : NetworkBehaviour
    {
        readonly SomeDictionaryIntString dictionary = new SomeDictionaryIntString();


        public class SomeDictionary<TKey, TItem> : SyncDictionary<TKey, TItem> { public SomeDictionary() : base(10) {} }

        public class SomeDictionaryIntString : SomeDictionary<int, string> { public SomeDictionaryIntString() : base() {} }
    }
}
