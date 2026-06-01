using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: player-proxy-character
    public class PlayerCharacter : NetworkBehaviour
    {
        private void Update()
        {
            // use HasAuthority, not IsLocalPlayer
            if (!HasAuthority)
                return;

            // handle input and movement
        }
    }
    // CodeEmbed-End: player-proxy-character
}
