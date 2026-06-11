using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: player-proxy-context
    public class PlayerContext : NetworkBehaviour
    {
        [SyncVar] public string PlayerName { get; set; }
        [SyncVar] public string Team { get; set; }

        // easy access to the gameplay character via NetworkPlayer.Identity
        [SyncVar] public PlayerCharacter ActiveCharacter { get; set; }
    }
    // CodeEmbed-End: player-proxy-context
}
