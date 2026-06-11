using UnityEngine;

namespace Mirage.Examples.MultipleAdditiveScenes
{
    public class PlayerScore : NetworkBehaviour
    {
        [SyncVar]
        public int playerNumber { get; set; }

        [SyncVar]
        public int scoreIndex { get; set; }

        [SyncVar]
        public int matchIndex { get; set; }

        [SyncVar]
        public uint score { get; set; }

        public int clientMatchIndex = -1;

        private void OnGUI()
        {
            if (!IsLocalPlayer && clientMatchIndex < 0)
                clientMatchIndex = Client.Player.Identity.GetComponent<PlayerScore>().matchIndex;

            if (IsLocalPlayer || matchIndex == clientMatchIndex)
                GUI.Box(new Rect(10f + (scoreIndex * 110), 10f, 100f, 25f), $"P{playerNumber}: {score}");
        }
    }
}
