using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryGenericAbstractInheritance
{
    class SyncDictionaryGenericAbstractInheritance : NetworkBehaviour
    {
        readonly SomeDictionaryIntString dictionary = new SomeDictionaryIntString();


        public abstract class SomeDictionary<TKey, TItem> : SyncDictionary<TKey, TItem> { protected SomeDictionary() : base(10) {} }

        public class SomeDictionaryIntString : SomeDictionary<int, string> { public SomeDictionaryIntString() : base() {} }
    }
}
