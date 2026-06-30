using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Analyzers
{
    namespace M1003.Triggering
    {
        // CodeEmbed-Start: mirage1003-triggering
        public class Player : NetworkBehaviour
        {
            // Error: ISyncObject field 'playerList' must be marked readonly and cannot be reassigned
            public SyncList<int> playerList = new SyncList<int>();

            public void ResetList()
            {
                playerList = new SyncList<int>();
            }
        }
        // CodeEmbed-End: mirage1003-triggering
    }

    namespace M1003.Resolved
    {
        // CodeEmbed-Start: mirage1003-resolved
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
        // CodeEmbed-End: mirage1003-resolved
    }
}
