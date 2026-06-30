using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1005.Triggering
    {
        // CodeEmbed-Start: mirage1005-triggering
        public class Player : NetworkBehaviour
        {
            // Error: SyncVar 'health' cannot be marked readonly
            [SyncVar]
            public readonly int health = 100;
        }
        // CodeEmbed-End: mirage1005-triggering
    }

    namespace M1005.Resolved
    {
        // CodeEmbed-Start: mirage1005-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: SyncVar is mutable at runtime
            [SyncVar]
            public int health = 100;
        }
        // CodeEmbed-End: mirage1005-resolved
    }
}
