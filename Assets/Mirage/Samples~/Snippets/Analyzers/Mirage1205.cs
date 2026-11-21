using Mirage;
using Cysharp.Threading.Tasks;

namespace Mirage.Snippets.Analyzers
{
    namespace M1205.Triggering
    {
        // CodeEmbed-Start: mirage1205-triggering
        using Mirage;
        using Cysharp.Threading.Tasks;

        public class Player : NetworkBehaviour
        {
            // Error: Observers target requires void return type
            [ClientRpc(target = RpcTarget.Observers)]
            public UniTask<int> RpcGetHealth()
            {
                return UniTask.FromResult(100);
            }

            // Error: Player target requires INetworkPlayer as first parameter
            [ClientRpc(target = RpcTarget.Player)]
            public void RpcGiveItem(int itemId)
            {
            }

            // Error: An Owner-targeted RPC cannot exclude its owner.
            [ClientRpc(target = RpcTarget.Owner, excludeOwner = true)]
            public void RpcNotifyOwner()
            {
            }
        }
        // CodeEmbed-End: mirage1205-triggering
    }

    namespace M1205.Resolved
    {
        // CodeEmbed-Start: mirage1205-resolved
        using Mirage;
        using Cysharp.Threading.Tasks;

        public class Player : NetworkBehaviour
        {
            // Correct: Owner target allows returning values
            [ClientRpc(target = RpcTarget.Owner)]
            public UniTask<int> RpcGetHealth()
            {
                return UniTask.FromResult(100);
            }

            // Correct: First parameter specifies the target player
            [ClientRpc(target = RpcTarget.Player)]
            public void RpcGiveItem(INetworkPlayer targetPlayer, int itemId)
            {
            }

            [ClientRpc(target = RpcTarget.Owner)]
            public void RpcNotifyOwner()
            {
            }
        }
        // CodeEmbed-End: mirage1205-resolved
    }
}
