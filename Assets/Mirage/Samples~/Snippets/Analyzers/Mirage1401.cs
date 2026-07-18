using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1401.Triggering
    {
        // CodeEmbed-Start: mirage1401-triggering
        public class PlayerHealth : NetworkBehaviour
        {
            [SyncVar]
            public int Health;

            private void Start()
            {
                // Warning: Accessing network state before object is spawned
                if (IsServer)
                    Health = 100;

                // Warning: Visibility throws an exception before spawn if no custom NetworkVisibility is attached
                var visibility = Identity.Visibility;

                // Warning: Helper properties are unsafe to access before spawn
                var owner = Owner;
                var isHost = IsHost;
                var isLocalClient = IsLocalClient;
                var isServerOnly = IsServerOnly;
                var isClientOnly = IsClientOnly;
            }
        }
        // CodeEmbed-End: mirage1401-triggering
    }

    namespace M1401.Resolved
    {
        // CodeEmbed-Start: mirage1401-resolved
        public class PlayerHealth : NetworkBehaviour
        {
            [SyncVar]
            public int Health;

            private void Awake()
            {
                Identity.OnStartServer.AddListener(OnStartServer);
            }

            // Correct: Run server initialization when the network server is ready
            public void OnStartServer()
            {
                Health = 100;

                // Correct: Access Visibility after the object is spawned
                var visibility = Identity.Visibility;
            }
        }
        // CodeEmbed-End: mirage1401-resolved
    }
}
