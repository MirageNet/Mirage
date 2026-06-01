using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1206.Triggering
    {
        // CodeEmbed-Start: mirage1206-triggering
        public class Player : NetworkBehaviour
        {
            // Warning: ServerRpc 'CmdFireWeapon' should have a [RateLimit] attribute to prevent spam
            [ServerRpc]
            public void CmdFireWeapon()
            {
            }
        }
        // CodeEmbed-End: mirage1206-triggering
    }

    namespace M1206.Resolved
    {
        // CodeEmbed-Start: mirage1206-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: ServerRpc decorated with [RateLimit] to throttle client requests
            [ServerRpc]
            [RateLimit(Interval = 0.2f, Refill = 5, MaxTokens = 10)]
            public void CmdFireWeapon()
            {
            }
        }
        // CodeEmbed-End: mirage1206-resolved
    }
}
