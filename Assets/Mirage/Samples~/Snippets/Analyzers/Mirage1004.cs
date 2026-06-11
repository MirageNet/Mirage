using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Analyzers
{
    namespace M1004.Triggering
    {
        // CodeEmbed-Start: mirage1004-triggering
        public class Player : NetworkBehaviour
        {
            // Error: ISyncObject field 'playerList' must be marked readonly and cannot be reassigned
            public SyncList<int> playerList = new SyncList<int>();

            public void ResetList()
            {
                playerList = new SyncList<int>();
            }
        }
        // CodeEmbed-End: mirage1004-triggering
    }

    namespace M1004.Resolved
    {
        // CodeEmbed-Start: mirage1004-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: Marked as readonly
            public readonly SyncList<int> playerList = new SyncList<int>();

            public void ResetList()
            {
                // Correct: Clear the list instead of reassigning it
                playerList.Clear();
            }
        }
        // CodeEmbed-End: mirage1004-resolved
    }
}
