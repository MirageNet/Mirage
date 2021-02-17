using Mirage;
using Mirage.Weaver.Extra;

namespace GeneratedReaderWriter.CreatesForStructFromDifferentAssemblies
{
    public class CreatesForStructFromDifferentAssemblies : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(SomeData data)
        {
            // empty
        }
    }
}
