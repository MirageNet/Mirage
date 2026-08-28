using UnityEngine;

namespace Mirage.Examples.MultipleAdditiveScenes
{
    public class PlayerScore : NetworkBehaviour
    {
        [SyncVar]
        public int playerNumber;

        [SyncVar]
        public int scoreIndex;

        [SyncVar]
        public int matchIndex;

        [SyncVar]
        public uint score;

        public int clientMatchIndex = -1;

        private void OnGUI()
        {
            // a server-only instance has no local player to compare matches with,
            // and on a client the local character might not be spawned yet
            if (!IsClient || !Client.Player.HasCharacter)
                return;

            if (!IsLocalPlayer && clientMatchIndex < 0)
                clientMatchIndex = Client.Player.Identity.GetComponent<PlayerScore>().matchIndex;

            if (IsLocalPlayer || matchIndex == clientMatchIndex)
                GUI.Box(new Rect(10f + (scoreIndex * 110), 10f, 100f, 25f), $"P{playerNumber}: {score}");
        }
    }
}
