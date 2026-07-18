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
            public NetworkIdentity Identity;

            private void Update()
            {
                // Warning: Checks Inspector component state instead of active network status
                if (Server.enabled)
                {
                    // Server logic...
                }

                if (Client.enabled)
                {
                    // Client logic...
                }

                if (Identity.enabled)
                {
                    // Identity logic...
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
            public NetworkIdentity Identity;

            private void Update()
            {
                // Correct: Checks if the server/client is actively running
                if (Server.Active)
                {
                    // Server logic...
                }

                if (Client.Active)
                {
                    // Client logic...
                }

                // Correct: Checks if the identity is active and spawned on the network
                if (Identity.IsSpawned)
                {
                    // Identity logic...
                }
            }
        }
        // CodeEmbed-End: mirage1403-resolved
    }
}
