using UnityEngine;
using Mirage;

namespace Mirage.Snippets.RemoteActions
{
    // CodeEmbed-Start: rate-limiting-emote
    public class PlayerSocial : NetworkBehaviour
    {
        // Allows 1 emote every 2 seconds, but the player can burst up to 3 emotes consecutively.
        // Spamming emotes too fast will drop the call and apply a penalty to their error limit.
        // Penalty set to 0 to not penalize the player for sending too many emotes, we can just ignore them.
        [ServerRpc]
        [RateLimit(Interval = 2f, Refill = 1, MaxTokens = 3, Penalty = 0)]
        public void CmdSendEmote(int emoteId)
        {
            // ... process and broadcast emote to other players ...
        }
    }
    // CodeEmbed-End: rate-limiting-emote

    // CodeEmbed-Start: rate-limiting-move
    public class PlayerRTSController : NetworkBehaviour
    {
        // Players should realistically only be issuing a few move commands a second.
        // We allow a healthy burst of 10 commands if they are furiously clicking across the map.
        // However, if a cheat sends 100 move instructions instantly, the large Penalty (10) 
        // will quickly exceed the global error threshold and kick them.
        [ServerRpc]
        [RateLimit(Interval = 1f, Refill = 5, MaxTokens = 10, Penalty = 10)]
        public void CmdMoveUnit(NetworkIdentity unit, Vector3 targetPosition)
        {
            // ... process movement command ...
        }
    }
    // CodeEmbed-End: rate-limiting-move
}
