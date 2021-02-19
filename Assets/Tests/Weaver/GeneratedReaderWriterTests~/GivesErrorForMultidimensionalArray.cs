using Mirage;

namespace GeneratedReaderWriter.GivesErrorForMultidimensionalArray
{
    public class GivesErrorForMultidimensionalArray : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcDoSomething(int[,] data)
        {
            // empty
        }
    }
}
