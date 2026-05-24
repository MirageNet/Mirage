using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour
{
    class SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour : NetworkBehaviour
    {
        readonly SomeSyncDictionary<int, string> someDictionary = new SomeSyncDictionary<int, string>();


        public class SomeSyncDictionary<TKey, TItem> : SyncDictionary<TKey, TItem> { public SomeSyncDictionary() : base(10) {} }
    }
}
