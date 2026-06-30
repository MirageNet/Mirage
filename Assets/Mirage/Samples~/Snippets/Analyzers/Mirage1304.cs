using Mirage;
using UnityEngine;

namespace Mirage.Snippets.Analyzers
{
    namespace M1304.Triggering
    {
        // CodeEmbed-Start: mirage1304-triggering
        public class LocalComponent : MonoBehaviour
        {
            public int health;
        }

        [NetworkMessage]
        public struct DamageMessage
        {
            // Error: MonoBehaviour type 'LocalComponent' is not serializable by Mirage.
            public LocalComponent target;
        }
        // CodeEmbed-End: mirage1304-triggering
    }

    namespace M1304.Resolved
    {
        // CodeEmbed-Start: mirage1304-resolved
        public class NetworkedComponent : NetworkBehaviour
        {
            public int health;
        }

        [NetworkMessage]
        public struct DamageMessage
        {
            // Correct: NetworkBehaviour is serializable by Mirage
            public NetworkedComponent target;
        }
        // CodeEmbed-End: mirage1304-resolved
    }
}
