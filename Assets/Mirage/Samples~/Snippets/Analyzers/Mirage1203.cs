using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1203.Triggering
    {
        // CodeEmbed-Start: mirage1203-triggering
        using Mirage;

        public class Player : NetworkBehaviour
        {
            // Error: RPC parameters cannot use ref, out, or in modifiers
            [ServerRpc]
            public void CmdTakeDamage(ref int health, in int damage)
            {
                health -= damage;
            }
        }
        // CodeEmbed-End: mirage1203-triggering
    }

    namespace M1203.Recommended
    {
        // CodeEmbed-Start: mirage1203-recommended
        using Mirage;

        public class Player : NetworkBehaviour
        {
            [SyncVar]
            public int Health;

            // Correct: Pass by value and synchronize via [SyncVar]
            [ServerRpc]
            public void CmdTakeDamage(int damage)
            {
                Health -= damage;
            }
        }
        // CodeEmbed-End: mirage1203-recommended
    }

    namespace M1203.Alternative
    {
        // CodeEmbed-Start: mirage1203-alternative
        using Mirage;
        using Cysharp.Threading.Tasks;

        public class Player : NetworkBehaviour
        {
            private int health = 100;

            [ServerRpc]
            public UniTask<int> CmdTakeDamage(int damage)
            {
                health -= damage; // Validate gameplay inputs before applying them.
                return UniTask.FromResult(health);
            }
        }
        // CodeEmbed-End: mirage1203-alternative
    }
}
