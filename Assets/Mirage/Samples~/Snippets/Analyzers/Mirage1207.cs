using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1207.Triggering
    {
        // CodeEmbed-Start: mirage1207-triggering
        using Mirage;

        public class Player : NetworkBehaviour
        {
            // Advisory: Review the request budget for this ServerRpc.
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
        using Mirage;

        public class Player : NetworkBehaviour
        {
            // Example budget: Ten initial tokens; refill five per 0.2 seconds.
            [ServerRpc]
            [RateLimit(Interval = 0.2f, Refill = 5, MaxTokens = 10)]
            public void CmdFireWeapon()
            {
            }
        }
        // CodeEmbed-End: mirage1207-resolved
    }
}
