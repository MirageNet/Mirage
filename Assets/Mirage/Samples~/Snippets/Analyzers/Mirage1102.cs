using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1102.Triggering
    {
        // CodeEmbed-Start: mirage1102-triggering
        public class PlayerCombat : NetworkBehaviour
        {
            // Redundant: [Server] is already implied by [ServerRpc]
            [Server]
            [ServerRpc]
            public void CmdFireWeapon(int weaponId)
            {
                // Weapon fire logic
            }

            // Redundant: [Client] is already implied by [ClientRpc]
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
