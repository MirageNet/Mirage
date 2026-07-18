using UnityEngine;
using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1101.Triggering
    {
        // CodeEmbed-Start: mirage1101-triggering
        public class GameManager : MonoBehaviour
        {
            // Error: 'SyncVarAttribute' cannot be used outside NetworkBehaviour
            [SyncVar]
            public int score;
        }
        // CodeEmbed-End: mirage1101-triggering
    }

    namespace M1101.Resolved
    {
        // CodeEmbed-Start: mirage1101-resolved
        public class GameManager : NetworkBehaviour
        {
            [SyncVar]
            public int score;
        }
        // CodeEmbed-End: mirage1101-resolved
    }
}
