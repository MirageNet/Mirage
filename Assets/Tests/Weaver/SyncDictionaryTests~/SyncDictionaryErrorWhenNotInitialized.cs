using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionaryErrorWhenNotInitialized
{
    class SyncDictionaryBehaviour : NetworkBehaviour
    {
        public SyncDictionary<int, string> Foo;
    }
}
