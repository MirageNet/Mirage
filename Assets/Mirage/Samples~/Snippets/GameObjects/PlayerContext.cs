using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: player-proxy-context
    public class PlayerContext : NetworkBehaviour
    {
        [SyncVar] public string PlayerName;
        [SyncVar] public string Team;

        // easy access to the gameplay character via NetworkPlayer.Identity
        [SyncVar] public PlayerCharacter ActiveCharacter;
    }
    // CodeEmbed-End: player-proxy-context
}
