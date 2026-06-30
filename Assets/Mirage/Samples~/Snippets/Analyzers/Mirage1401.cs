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
                // Warning: Accessing Network State (IsServer) in Awake/Start
                if (IsServer)
                    Health = 100;

                // Warning: Accessing Visibility before the object is spawned will throw an exception if no custom NetworkVisibility component is attached
                var visibility = Identity.Visibility;
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

            // Correct: Run server initialization when the network server has started
            public void OnStartServer()
            {
                Health = 100;

                // Correct: Access Visibility once the object has been spawned
                var visibility = Identity.Visibility;
            }
        }
        // CodeEmbed-End: mirage1401-resolved
    }
}
