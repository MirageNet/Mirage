using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Analyzers
{
    namespace M1003.Triggering
    {
        // CodeEmbed-Start: mirage1003-triggering
        public class Player : NetworkBehaviour
        {
            // Analyzer error: protect the registered instance with readonly.
            public SyncList<int> playerList = new SyncList<int>();

            public void ResetList()
            {
                playerList = new SyncList<int>(); // The original list remains registered.
            }
        }
        // CodeEmbed-End: mirage1003-triggering
    }

    namespace M1003.Resolved
    {
        // CodeEmbed-Start: mirage1003-resolved
        public class Player : NetworkBehaviour
        {
            // The reference is fixed; the collection contents can still change.
            public readonly SyncList<int> playerList = new SyncList<int>();

            public void ResetList()
            {
                // Call on the sending side; preserve the registered instance.
                playerList.Clear();
            }
        }
        // CodeEmbed-End: mirage1003-resolved
    }
}
