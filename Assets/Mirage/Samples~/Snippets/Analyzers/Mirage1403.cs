using Mirage;
using UnityEngine;

namespace Mirage.Snippets.Analyzers
{
    namespace M1403.Triggering
    {
        // CodeEmbed-Start: mirage1403-triggering
        public class ServerStatusCheck : MonoBehaviour
        {
            public NetworkServer Server;
            public NetworkClient Client;

            private void Update()
            {
                // Warning: Checking .enabled checks Unity Component status, not if the server/client is active
                if (Server.enabled)
                {
                    // Server logic...
                }

                if (Client.enabled)
                {
                    // Client logic...
                }
            }
        }
        // CodeEmbed-End: mirage1403-triggering
    }

    namespace M1403.Resolved
    {
        // CodeEmbed-Start: mirage1403-resolved
        public class ServerStatusCheck : MonoBehaviour
        {
            public NetworkServer Server;
            public NetworkClient Client;

            private void Update()
            {
                // Correct: Use .Active to check if the server/client is actively running
                if (Server.Active)
                {
                    // Server logic...
                }

                if (Client.Active)
                {
                    // Client logic...
                }
            }
        }
        // CodeEmbed-End: mirage1403-resolved
    }
}
