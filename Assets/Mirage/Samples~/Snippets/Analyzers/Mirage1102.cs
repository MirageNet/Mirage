using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1102.Triggering
    {
        // CodeEmbed-Start: mirage1102-triggering
        public class PlayerCombat : NetworkBehaviour
        {
            // Redundant: the ServerRpc body already executes on the server.
            [Server]
            [ServerRpc]
            public void CmdFireWeapon(int weaponId)
            {
                // Weapon fire logic
            }

            // Redundant: the ClientRpc body already executes on a receiving client.
            [Client]
            [ClientRpc]
            public void RpcPlayExplosion(UnityEngine.Vector3 position)
            {
                // Play explosion effect
            }
        }
        // CodeEmbed-End: mirage1102-triggering
    }

    namespace M1102.Resolved
    {
        // CodeEmbed-Start: mirage1102-resolved
        public class PlayerCombat : NetworkBehaviour
        {
            [ServerRpc]
            public void CmdFireWeapon(int weaponId)
            {
                // Weapon fire logic
            }

            [ClientRpc]
            public void RpcPlayExplosion(UnityEngine.Vector3 position)
            {
                // Play explosion effect
            }
        }
        // CodeEmbed-End: mirage1102-resolved
    }
}
