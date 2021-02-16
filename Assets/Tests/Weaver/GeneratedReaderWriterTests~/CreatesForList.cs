using System.Collections.Generic;
using Mirage;

namespace GeneratedReaderWriter.CreatesForList
{
    public class CreatesForList : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(List<int> data)
        {
            // empty
        }
    }
}
