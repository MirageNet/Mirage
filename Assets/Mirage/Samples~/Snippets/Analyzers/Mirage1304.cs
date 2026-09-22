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
            // No default serializer or custom serializer for LocalComponent.
            public LocalComponent target;
        }
        // CodeEmbed-End: mirage1304-triggering
    }

    namespace M1304.Recommended
    {
        // CodeEmbed-Start: mirage1304-recommended
        public class NetworkedComponent : NetworkBehaviour
        {
            public int health;
        }

        [NetworkMessage]
        public struct DamageMessage
        {
            // Sends a reference to a spawned network object and its component.
            public NetworkedComponent target;
        }
        // CodeEmbed-End: mirage1304-recommended
    }

    namespace M1304.Alternative
    {
        // CodeEmbed-Start: mirage1304-alternative
        [NetworkMessage]
        public struct DamageMessage
        {
            // Correct: Pass a serializable identifier instead.
            public uint targetId;
        }
        // CodeEmbed-End: mirage1304-alternative
    }
}
