using Mirage;
using Cysharp.Threading.Tasks;

namespace Mirage.Snippets.Analyzers
{
    public struct PlayerStats {}

    namespace M1202.Triggering
    {
        // CodeEmbed-Start: mirage1202-triggering
        public class Player : NetworkBehaviour
        {
            // Errors: RPC method 'CmdTakeDamage' is invalid: cannot have generic parameters.
            [ServerRpc]
            public void CmdTakeDamage<T>(T damage)
            {
            }

            // Errors: RPC method 'CmdGetStats' is invalid: cannot return 'PlayerStats' (must return void or UniTask).
            [ServerRpc]
            public PlayerStats CmdGetStats()
            {
                return new PlayerStats();
            }
        }
        // CodeEmbed-End: mirage1202-triggering
    }

    namespace M1202.Resolved
    {
        // CodeEmbed-Start: mirage1202-resolved
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
        // CodeEmbed-End: mirage1202-resolved
    }
}
