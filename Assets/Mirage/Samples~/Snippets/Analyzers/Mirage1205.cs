using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1205.Triggering
    {
        // CodeEmbed-Start: mirage1205-triggering
        public class Player : NetworkBehaviour
        {
            // Error: RateLimit interval must be greater than zero, and MaxTokens must be >= Refill
            [ServerRpc]
            [RateLimit(Interval = -0.5f, Refill = 10, MaxTokens = 5)]
            public void CmdSpammyAction()
            {
            }
        }
        // CodeEmbed-End: mirage1205-triggering
    }

    namespace M1205.Resolved
    {
        // CodeEmbed-Start: mirage1205-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: Positive interval and MaxTokens >= Refill
            [ServerRpc]
            [RateLimit(Interval = 1.0f, Refill = 10, MaxTokens = 20)]
            public void CmdSpammyAction()
            {
            }
        }
        // CodeEmbed-End: mirage1205-resolved
    }
}
