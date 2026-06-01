using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1203.Triggering
    {
        // CodeEmbed-Start: mirage1203-triggering
        public class Player : NetworkBehaviour
        {
            // Error: ServerRpc method 'CmdSpawnGlobal' must not be static
            [ServerRpc]
            public static void CmdSpawnGlobal()
            {
                // Static context has no NetworkIdentity
            }
        }
        // CodeEmbed-End: mirage1203-triggering
    }

    namespace M1203.Resolved
    {
        // CodeEmbed-Start: mirage1203-resolved
        public class Player : NetworkBehaviour
        {
            // Correct: Instance method has access to the NetworkBehaviour state
            [ServerRpc]
            public void CmdSpawn()
            {
                // Normal instance context
            }
        }
        // CodeEmbed-End: mirage1203-resolved
    }
}
