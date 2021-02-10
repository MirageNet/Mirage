using System;
using Mirage;

namespace GeneratedReaderWriter.CreatesForArraySegment
{
    public class CreatesForArraySegment : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(ArraySegment<int> data)
        {
            // empty
        }
    }
}
