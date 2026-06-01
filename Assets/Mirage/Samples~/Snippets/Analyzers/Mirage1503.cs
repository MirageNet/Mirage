using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Analyzers
{
    namespace M1503.Triggering
    {
        // CodeEmbed-Start: mirage1503-triggering
        public class Player : NetworkBehaviour
        {
            // Warning: 'Health' uses uncompressed int which has high bit-overhead.
            [SyncVar]
            public int Health { get; set; }

            // Warning: 'PlayerScale' uses uncompressed float which has high bit-overhead.
            [SyncVar]
            public float PlayerScale { get; set; }
        }
        // CodeEmbed-End: mirage1503-triggering
    }

    namespace M1503.Resolved
    {
        // CodeEmbed-Start: mirage1503-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: Restrict Health to 7 bits (0-127 range)
            [SyncVar, BitCount(7)]
            public int Health { get; set; }

            // Correct: Compress float with a defined range and precision
            [SyncVar, FloatPack(-10f, 10f, 0.01f)]
            public float PlayerScale { get; set; }
        }
        // CodeEmbed-End: mirage1503-resolved
    }
}
