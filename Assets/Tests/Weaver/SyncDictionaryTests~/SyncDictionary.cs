using Mirage;
using Mirage.Collections;

namespace SyncDictionaryTests.SyncDictionary
{
    class SyncDictionaryValid : NetworkBehaviour
    {
        public class SyncDictionaryIntString : SyncDictionary<int, string> { }

        public SyncDictionaryIntString Foo;
    }


}
