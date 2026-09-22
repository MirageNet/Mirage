using Mirage;
using Cysharp.Threading.Tasks;

namespace Mirage.Snippets.Analyzers
{
    namespace M1202.Triggering
    {
        // CodeEmbed-Start: mirage1202-triggering
        using Mirage;
        using Cysharp.Threading.Tasks;

        public struct PlayerStats { public int Health; }

        public abstract class Player : NetworkBehaviour
        {
            // Error: Choose a single RPC direction.
            [ServerRpc, ClientRpc]
            public void RpcBothDirections() { }

            // Error: Serialized parameters cannot be optional.
            [ServerRpc]
            public void CmdOptionalDamage(int damage = 1) { }

            // Error: RPC methods cannot be abstract.
            [ServerRpc]
            public abstract void CmdReload();

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
        using Mirage;
        using Cysharp.Threading.Tasks;

        public struct PlayerStats { public int Health; }
        public struct DamageContainer<T> { public T Value; }

        // Allowed: Generic NetworkBehaviour class.
        public class Player<T> : NetworkBehaviour
        {
            // RPC handling supplies the optional sender; it is not sent as payload.
            [ServerRpc]
            public void CmdDamage(int damage, INetworkPlayer sender = null) { }

            // The concrete type used for T still needs registered readers and writers.
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
            public virtual void CmdReload()
            {
            }

            [ServerRpc]
            public async UniTask<PlayerStats> CmdGetStats()
            {
                await UniTask.Yield();
                return new PlayerStats { Health = 100 };
            }
        }

        // int already has serializers.
        public class IntPlayer : Player<int> { }
        // CodeEmbed-End: mirage1202-resolved
    }
}
