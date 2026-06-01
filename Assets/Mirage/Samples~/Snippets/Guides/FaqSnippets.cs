using Mirage;
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

        struct MyCustomStruct
        {
            int someNumber;
            Vector3 somePosition;
        }
        // CodeEmbed-End: faq-custom-data
    }
}
