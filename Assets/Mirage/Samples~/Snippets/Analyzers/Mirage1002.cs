using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Analyzers
{
    namespace M1002.Triggering
    {
        // CodeEmbed-Start: mirage1002-triggering
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
        // CodeEmbed-End: mirage1002-triggering
    }

    namespace M1002.Resolved
    {
        // CodeEmbed-Start: mirage1002-resolved
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
        // CodeEmbed-End: mirage1002-resolved
    }

    namespace M1002.ResolvedClass
    {
        // CodeEmbed-Start: mirage1002-resolved-class
        public class PlayerData
        {
            public int health;
        }

        public class Player : NetworkBehaviour
        {
            public readonly SyncList<PlayerData> playerList = new SyncList<PlayerData>();

            public void DamagePlayer(int index, int damage)
            {
                // Correct: Mutating the class object directly, then calling SetItemDirtyAt to sync changes
                playerList[index].health -= damage;
                playerList.SetItemDirtyAt(index);
            }
        }
        // CodeEmbed-End: mirage1002-resolved-class
    }
}

