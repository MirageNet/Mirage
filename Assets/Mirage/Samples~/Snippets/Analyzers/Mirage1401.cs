namespace Mirage.Snippets.Analyzers
{
    namespace M1401.Triggering
    {
        // CodeEmbed-Start: mirage1401-triggering
        public class PlayerHealth : NetworkBehaviour
        {
            [SyncVar]
            public int Health { get; set; }

            private void Start()
            {
                // Warning: Accessing Network State (IsServer) in Awake/Start
                if (IsServer)
                    Health = 100;
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
            public int Health { get; set; }

            private void Awake()
            {
                Identity.OnStartServer.AddListener(OnStartServer);
            }

            // Correct: Run server initialization when the network server has started
            public void OnStartServer()
            {
                Health = 100;
            }
        }
        // CodeEmbed-End: mirage1401-resolved
    }
}
