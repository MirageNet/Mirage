using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1203.Triggering
    {
        // CodeEmbed-Start: mirage1203-triggering
        public class Player : NetworkBehaviour
        {
            // Error: ServerRpc method 'CmdTakeDamage' cannot have ref/out/in parameters
            [ServerRpc]
            public void CmdTakeDamage(ref int health, in int damage)
            {
                health -= damage;
            }
        }
        // CodeEmbed-End: mirage1203-triggering
    }

    namespace M1203.Resolved
    {
        // CodeEmbed-Start: mirage1203-resolved
        public class Player : NetworkBehaviour
        {
            [SyncVar]
            public int Health;

            // Correct: Pass by value and synchronize via SyncVar
            [ServerRpc]
            public void CmdTakeDamage(int damage)
            {
                Health -= damage;
            }
        }
        // CodeEmbed-End: mirage1203-resolved
    }
}
