using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1206.Triggering
    {
        // CodeEmbed-Start: mirage1206-triggering
        using Mirage;

        public class Player : NetworkBehaviour
        {
            // Weaver errors: Interval <= 0, MaxTokens <= 0, and Penalty < 0.
            [ServerRpc]
            [RateLimit(Interval = -0.5f, Refill = 10, MaxTokens = 0, Penalty = -1)]
            public void CmdSpammyAction()
            {
            }

            // NaN can stop the limiter from rejecting excess calls.
            [ServerRpc]
            [RateLimit(Interval = float.NaN)]
            public void CmdInvalidInterval()
            {
            }
        }
        // CodeEmbed-End: mirage1206-triggering
    }

    namespace M1206.Resolved
    {
        // CodeEmbed-Start: mirage1206-resolved
        using Mirage;

        public class Player : NetworkBehaviour
        {
            // Valid: Refill is capped at MaxTokens; MaxTokens may be smaller.
            [ServerRpc]
            [RateLimit(Interval = 1.0f, Refill = 10, MaxTokens = 5, Penalty = 0)]
            public void CmdSpammyAction()
            {
            }
        }
        // CodeEmbed-End: mirage1206-resolved
    }
}
