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
            // Proposed diagnostic with default serializers:
            // 'PlayerUpdateMessage' payload: 13-17 bytes (excludes message ID).
        }
        // CodeEmbed-End: mirage1501-example
    }
}
