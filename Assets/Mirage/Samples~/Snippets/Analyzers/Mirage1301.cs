using Mirage;
using System.Threading;

namespace Mirage.Snippets.Analyzers
{
    namespace M1301.Triggering
    {
        // CodeEmbed-Start: mirage1301-triggering
        [NetworkMessage]
        public struct StartSessionMessage
        {
            // Error: Field type 'Thread' is not serializable by Mirage.
            public Thread executionThread;
        }
        // CodeEmbed-End: mirage1301-triggering
    }

    namespace M1301.Resolved
    {
        // CodeEmbed-Start: mirage1301-resolved
        [NetworkMessage]
        public struct StartSessionMessage
        {
            // Correct: Pass a serializable identifier instead of the raw thread object
            public string threadName;
        }
        // CodeEmbed-End: mirage1301-resolved
    }
}
