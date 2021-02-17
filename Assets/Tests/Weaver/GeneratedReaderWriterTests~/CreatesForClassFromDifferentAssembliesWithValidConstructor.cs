using Mirage;
using Mirage.Weaver.Extra;

namespace GeneratedReaderWriter.CreatesForClassFromDifferentAssembliesWithValidConstructor
{
    public class CreatesForClassFromDifferentAssembliesWithValidConstructor : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(SomeDataClassWithConstructor data)
        {
            // empty
        }
    }
}
