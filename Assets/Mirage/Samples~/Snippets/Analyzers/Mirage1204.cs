using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1204.Triggering
    {
        // CodeEmbed-Start: mirage1204-triggering
        public class Player : NetworkBehaviour
        {
            // Error: ServerRpc method 'CmdSpawnGlobal' must not be static
            [ServerRpc]
            public static void CmdSpawnGlobal()
            {
            }
        }
        // CodeEmbed-End: mirage1204-triggering
    }

    namespace M1204.Resolved
    {
        // CodeEmbed-Start: mirage1204-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: Instance method has access to the NetworkBehaviour state
            [ServerRpc]
            public void CmdSpawn()
            {
            }
        }
        // CodeEmbed-End: mirage1204-resolved
    }
}
