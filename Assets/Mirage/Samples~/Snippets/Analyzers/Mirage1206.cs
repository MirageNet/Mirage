using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1206.Triggering
    {
        // CodeEmbed-Start: mirage1206-triggering
        public class Player : NetworkBehaviour
        {
            // Error: RateLimit interval must be greater than zero, MaxTokens must be >= Refill, and Penalty must be >= 0
            [ServerRpc]
            [RateLimit(Interval = -0.5f, Refill = 10, MaxTokens = 5, Penalty = -1)]
            public void CmdSpammyAction()
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
            // Correct: Positive interval, MaxTokens >= Refill, and Penalty >= 0
            [ServerRpc]
            [RateLimit(Interval = 1.0f, Refill = 10, MaxTokens = 20, Penalty = 1)]
            public void CmdSpammyAction()
            {
            }
        }
        // CodeEmbed-End: mirage1206-resolved
    }
}
