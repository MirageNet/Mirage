using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1202.Triggering
    {
        // CodeEmbed-Start: mirage1202-triggering
        public class Player : NetworkBehaviour
        {
            // Error: ServerRpc method 'CmdTakeDamage' cannot have ref/out parameters
            [ServerRpc]
            public void CmdTakeDamage(ref int health)
            {
                health -= 10;
            }
        }
        // CodeEmbed-End: mirage1202-triggering
    }

    namespace M1202.Resolved
    {
        // CodeEmbed-Start: mirage1202-resolved
        public class Player : NetworkBehaviour
        {
            [SyncVar]
            public int Health { get; set; }

            // Correct: Pass by value and synchronize via SyncVar
            [ServerRpc]
            public void CmdTakeDamage(int damage)
            {
                Health -= damage;
            }
        }
        // CodeEmbed-End: mirage1202-resolved
    }
}
