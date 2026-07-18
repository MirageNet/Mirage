using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1207.Triggering
    {
        // CodeEmbed-Start: mirage1207-triggering
        public class Player : NetworkBehaviour
        {
            // Warning: ServerRpc lacks [RateLimit] to prevent client spam
            [ServerRpc]
            public void CmdFireWeapon()
            {
            }
        }
        // CodeEmbed-End: mirage1207-triggering
    }

    namespace M1207.Resolved
    {
        // CodeEmbed-Start: mirage1207-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: ServerRpc uses [RateLimit] to throttle requests
            [ServerRpc]
            [RateLimit(Interval = 0.2f, Refill = 5, MaxTokens = 10)]
            public void CmdFireWeapon()
            {
            }
        }
        // CodeEmbed-End: mirage1207-resolved
    }
}
