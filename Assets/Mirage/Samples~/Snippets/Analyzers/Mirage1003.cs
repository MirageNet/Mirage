using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Analyzers
{
    namespace M1003.Triggering
    {
        // CodeEmbed-Start: mirage1003-triggering
        public class PlayerData
        {
            public int health;
        }

        public class Player : NetworkBehaviour
        {
            public readonly SyncList<PlayerData> playerList = new SyncList<PlayerData>();

            public void DamagePlayer(int index, int damage)
            {
                // Warning: Direct mutation of elements inside playerList is not supported because changes cannot be tracked.
                playerList[index].health -= damage;
            }
        }
        // CodeEmbed-End: mirage1003-triggering
    }

    namespace M1003.Resolved
    {
        // CodeEmbed-Start: mirage1003-resolved
        public struct PlayerData
        {
            public int health;
        }

        public class Player : NetworkBehaviour
        {
            public readonly SyncList<PlayerData> playerList = new SyncList<PlayerData>();

            public void DamagePlayer(int index, int damage)
            {
                // Correct: Modifying the element and setting it back, triggering the index setter
                var data = playerList[index];
                data.health -= damage;
                playerList[index] = data;
            }
        }
        // CodeEmbed-End: mirage1003-resolved
    }
}
