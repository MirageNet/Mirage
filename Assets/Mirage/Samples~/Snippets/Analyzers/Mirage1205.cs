using Mirage;
using Cysharp.Threading.Tasks;

namespace Mirage.Snippets.Analyzers
{
    namespace M1205.Triggering
    {
        // CodeEmbed-Start: mirage1205-triggering
        public class Player : NetworkBehaviour
        {
            // Error: [ClientRpc] must return void when target is Observers.
            [ClientRpc(target = RpcTarget.Observers)]
            public UniTask<int> RpcGetHealth()
            {
                return UniTask.FromResult(100);
            }

            // Error: ClientRpc method with target = Player requires first parameter to be INetworkPlayer
            [ClientRpc(target = RpcTarget.Player)]
            public void RpcGiveItem(int itemId)
            {
            }
        }
        // CodeEmbed-End: mirage1205-triggering
    }

    namespace M1205.Resolved
    {
        // CodeEmbed-Start: mirage1205-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: Targeted RPC returning value to the Owner
            [ClientRpc(target = RpcTarget.Owner)]
            public UniTask<int> RpcGetHealth()
            {
                return UniTask.FromResult(100);
            }

            // Correct: First parameter is the target player connection
            [ClientRpc(target = RpcTarget.Player)]
            public void RpcGiveItem(INetworkPlayer targetPlayer, int itemId)
            {
            }
        }
        // CodeEmbed-End: mirage1205-resolved
    }
}
