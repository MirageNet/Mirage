using UnityEngine;
using Mirage;

namespace Mirage.Snippets.RemoteActions.ClientRpcSimple
{
    // CodeEmbed-Start: client-rpc-attribute
    public class MyClientRpcExampleBehaviour : NetworkBehaviour
    {
        [ClientRpc]
        public void MyRpcFunction() 
        {
            // Code to invoke on client
        }
    }
    // CodeEmbed-End: client-rpc-attribute
}

namespace Mirage.Snippets.RemoteActions.ClientRpcPlayer
{
    // CodeEmbed-Start: client-rpc-player
    public class MyClientRpcExampleBehaviour : NetworkBehaviour
    {
        [ClientRpc(target = RpcTarget.Player)]
        public void MyRpcFunction(NetworkPlayer target) 
        {
            // Code to invoke on client
        }
    }
    // CodeEmbed-End: client-rpc-player
}

namespace Mirage.Snippets.RemoteActions.ClientRpcHealth
{
    // CodeEmbed-Start: client-rpc-example-health
    public class Player : NetworkBehaviour
    {
        private int health;

        public void TakeDamage(int amount)
        {
            if (!IsServer)
                return;

            health -= amount;
            Damage(amount);
        }

        [ClientRpc]
        private void Damage(int amount)
        {
            Debug.Log("Took damage:" + amount);
        }
    }
    // CodeEmbed-End: client-rpc-example-health
}

namespace Mirage.Snippets.RemoteActions.ClientRpcMagic
{
    // CodeEmbed-Start: client-rpc-example-magic
    public class Player : NetworkBehaviour
    {
        private int health;

        [Server]
        private void Magic(GameObject target, int damage)
        {
            target.GetComponent<Player>().health -= damage;

            NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
            DoMagic(opponentIdentity.Owner, damage);
        }

        [ClientRpc(target = RpcTarget.Player)]
        public void DoMagic(INetworkPlayer target, int damage)
        {
            // This will appear on the opponent's client, not the attacking player's
            Debug.Log($"Magic Damage = {damage}");
        }

        [Server]
        private void HealMe()
        {
            health += 10;
            Healed(10);
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public void Healed(int amount)
        {
            // No NetworkPlayer parameter, so it goes to owner
            Debug.Log($"Health increased by {amount}");
        }
    }
    // CodeEmbed-End: client-rpc-example-magic
}
