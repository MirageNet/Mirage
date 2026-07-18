using Mirage;
using UnityEngine;

namespace Mirage.Snippets.Analyzers
{
    namespace M1501.Example
    {
        // CodeEmbed-Start: mirage1501-example
        [NetworkMessage]
        public struct PlayerUpdateMessage
        {
            public int id;
            public Vector3 position;
            // Diagnostic: 'PlayerUpdateMessage' estimated size: 13 bytes.
        }
        // CodeEmbed-End: mirage1501-example
    }
}
