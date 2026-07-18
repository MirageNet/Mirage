using Mirage;
using Cysharp.Threading.Tasks;

namespace Mirage.Snippets.Analyzers
{
    public struct PlayerStats {}
    public struct DamageContainer<T> { public T Value; }

    namespace M1202.Triggering
    {
        // CodeEmbed-Start: mirage1202-triggering
        public class Player : NetworkBehaviour
        {
            // Error: RPC methods cannot declare generic parameters.
            [ServerRpc]
            public void CmdTakeDamage<T>(T damage)
            {
            }

            // Error: RPC methods must return void or UniTask<T>.
            [ServerRpc]
            public PlayerStats CmdGetStats()
            {
                return new PlayerStats();
            }

            // Error: RPC methods must return void or UniTask<T> (non-generic UniTask is not supported).
            [ServerRpc]
            public UniTask CmdDoSomethingAsync()
            {
                return UniTask.CompletedTask;
            }
        }
        // CodeEmbed-End: mirage1202-triggering
    }

    namespace M1202.Resolved
    {
        // CodeEmbed-Start: mirage1202-resolved
        // Allowed: Generic NetworkBehaviour class.
        public class Player<T> : NetworkBehaviour
        {
            // Allowed: Using generic parameters from the enclosing class.
            [ServerRpc]
            public void CmdProcessGenericArg(T data)
            {
            }

            // Allowed: Using closed generic types.
            [ServerRpc]
            public void CmdTakeDamage(DamageContainer<int> damage)
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
