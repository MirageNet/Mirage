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
                // Warning: This nested mutation does not queue a collection update.
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
                // Call on the sending side. The changed struct compares different.
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
                // Call on the sending side, then explicitly queue this element.
                playerList[index].health -= damage;
                playerList.SetItemDirtyAt(index);
            }
        }
        // CodeEmbed-End: mirage1002-resolved-class
    }

    namespace M1002.ResolvedDictionary
    {
        // CodeEmbed-Start: mirage1002-resolved-dictionary
        public class PlayerData
        {
            public int health;
        }

        public class Player : NetworkBehaviour
        {
            public readonly SyncDictionary<int, PlayerData> players = new SyncDictionary<int, PlayerData>();

            public void DamagePlayer(int playerId, int damage)
            {
                // Call on the sending side. Dictionary assignment queues an update.
                var data = players[playerId];
                data.health -= damage;
                players[playerId] = data;
            }
        }
        // CodeEmbed-End: mirage1002-resolved-dictionary
    }
}
