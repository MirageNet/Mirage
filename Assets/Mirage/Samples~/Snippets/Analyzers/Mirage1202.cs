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
            // Error: RPC methods cannot define their own generic parameters.
            [ServerRpc]
            public void CmdTakeDamage<T>(T damage)
            {
            }

            // Error: RPC methods must return void, UniTask, or UniTask<T>.
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
        // Allowed: NetworkBehaviours can be generic classes.
        public class Player<T> : NetworkBehaviour
        {
            // Allowed: RPC methods can use generic parameters from the enclosing class.
            [ServerRpc]
            public void CmdProcessGenericArg(T data)
            {
            }

            // Allowed: RPC methods can use closed generic types (e.g. DamageContainer<int>).
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
