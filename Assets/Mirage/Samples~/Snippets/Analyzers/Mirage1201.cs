using Mirage;
using Cysharp.Threading.Tasks;

namespace Mirage.Snippets.Analyzers
{
    public struct PlayerStats {}

    namespace M1201.Triggering
    {
        // CodeEmbed-Start: mirage1201-triggering
        public class Player : NetworkBehaviour
        {
            // Errors: RPC method 'CmdTakeDamage' is invalid: cannot have generic parameters.
            [ServerRpc]
            public void CmdTakeDamage<T>(T damage)
            {
            }

            // Errors: RPC method 'CmdGetStats' is invalid: cannot return 'PlayerStats'...
            [ServerRpc]
            public PlayerStats CmdGetStats()
            {
                return new PlayerStats();
            }
        }
        // CodeEmbed-End: mirage1201-triggering
    }

    namespace M1201.Resolved
    {
        // CodeEmbed-Start: mirage1201-resolved
        public class Player : NetworkBehaviour
        {
            [ServerRpc]
            public void CmdTakeDamage(int damage)
            {
            }

            [ServerRpc]
            public async UniTask<PlayerStats> CmdGetStats()
            {
                await UniTask.Yield();
                return new PlayerStats();
            }
        }
        // CodeEmbed-End: mirage1201-resolved
    }
}
