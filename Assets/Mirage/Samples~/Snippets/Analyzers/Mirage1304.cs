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
            // Error: MonoBehaviour components are not network-serializable.
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
            // Correct: NetworkBehaviour components are network-serializable.
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
