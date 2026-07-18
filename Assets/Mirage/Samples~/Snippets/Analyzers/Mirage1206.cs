using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1206.Triggering
    {
        // CodeEmbed-Start: mirage1206-triggering
        public class Player : NetworkBehaviour
        {
            // Error: Invalid settings (Interval <= 0, MaxTokens < Refill, Penalty < 0)
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
            // Correct: Valid settings (Interval > 0, MaxTokens >= Refill, Penalty >= 0)
            [ServerRpc]
            [RateLimit(Interval = 1.0f, Refill = 10, MaxTokens = 20, Penalty = 1)]
            public void CmdSpammyAction()
            {
            }
        }
        // CodeEmbed-End: mirage1206-resolved
    }
}
