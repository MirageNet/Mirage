using Mirage;
using UnityEngine;

namespace Mirage.Snippets.Analyzers
{
    namespace M1501.Triggering
    {
        // CodeEmbed-Start: mirage1501-triggering
        [NetworkMessage]
        public struct LargeTelemetryMessage
        {
            // Warning: Array of Vector3 has estimated size of 1536 bytes, exceeding the 1200-byte MTU limit.
            public Vector3[] historicalTransforms;
        }
        // CodeEmbed-End: mirage1501-triggering
    }

    namespace M1501.Resolved
    {
        // CodeEmbed-Start: mirage1501-resolved
        [NetworkMessage]
        public struct TelemetryUpdateMessage
        {
            public int sequenceNumber;
            // Correct: Send individual updates frequently instead of a large history array.
            public Vector3 position;
            public Quaternion rotation;
        }
        // CodeEmbed-End: mirage1501-resolved
    }
}
