using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1002.Triggering
    {
        // CodeEmbed-Start: mirage1002-triggering
        public class Player : NetworkBehaviour
        {
            private int _health;

            // Errors: SyncVar property 'health' must be a non-static auto-property...
            [SyncVar]
            public int health
            {
                get => _health;
                set => _health = value;
            }
        }
        // CodeEmbed-End: mirage1002-triggering
    }

    namespace M1002.Resolved
    {
        // CodeEmbed-Start: mirage1002-resolved
        public class Player : NetworkBehaviour
        {
            [SyncVar]
            public int health { get; set; }
        }
        // CodeEmbed-End: mirage1002-resolved
    }
}
