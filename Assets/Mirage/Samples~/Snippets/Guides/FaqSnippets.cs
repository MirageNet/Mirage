using UnityEngine;

namespace Mirage.Snippets.Guides
{
    public class FaqSnippets : NetworkBehaviour
    {
        // CodeEmbed-Start: faq-custom-data
        [ClientRpc]
        public void RpcDoSomething(MyCustomStruct data)
        {
            // do stuff here
        }

        public struct MyCustomStruct
        {
            public int someNumber;
            public Vector3 somePosition;
        }
        // CodeEmbed-End: faq-custom-data
    }
}
