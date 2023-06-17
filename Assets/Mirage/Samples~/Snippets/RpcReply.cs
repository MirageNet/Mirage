using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.Snippets.ClientRpcReply
{
    // CodeEmbed-Start: client-rpc-reply
    public class SelectCharacter : NetworkBehaviour
    {
        // need to target owner so that we can reply to them
        [ClientRpc(target = RpcTarget.Owner)]
        public UniTask<string> GetCharacter()
        {
            // return the name of the character that the user selected
            return UniTask.FromResult("myCharacter");
        }

        [Client]
        public async UniTaskVoid WaitForPlayerToSelectCharacter()
        {
            // Call the RPC and wait for the response without blocking the main thread
            var characterName = await GetCharacter();

            Debug.Log($"Selected Character {characterName}");
        }
    }
    // CodeEmbed-End: client-rpc-reply


    // CodeEmbed-Start: server-rpc-reply
    public class Shop : NetworkBehaviour
    {
        [ServerRpc]
        public UniTask<int> GetPrice(string item)
        {
            // do some loop up to get the price of some item
            // this can be from a database, or settings file or whatever
            switch (item)
            {
                case "turnip":
                    return UniTask.FromResult(10);
                case "apple":
                    return UniTask.FromResult(3);
                default:
                    return UniTask.FromResult(int.MaxValue);
            }
        }

        [Client]
        public async UniTaskVoid DisplayTurnipPrice()
        {
            // Call the RPC and wait for the response without blocking the main thread
            var price = await GetPrice("turnip");
            Debug.Log($"Turnips price {price}");
        }
    }
    // CodeEmbed-End: server-rpc-reply
}
