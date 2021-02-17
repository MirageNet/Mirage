using Mirage;
using Mirage.Weaver.Extra;

namespace GeneratedReaderWriter.CreatesForClassFromDifferentAssemblies
{
    public class CreatesForClassFromDifferentAssemblies : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(SomeDataClass data)
        {
            // empty
        }
    }
}
