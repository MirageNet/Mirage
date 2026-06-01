using Mirage;
using System.Threading;

namespace Mirage.Snippets.Analyzers
{
    namespace M1302.Triggering
    {
        // CodeEmbed-Start: mirage1302-triggering
        [NetworkMessage]
        public struct StartSessionMessage
        {
            // Error: Field type 'Thread' is not serializable by Mirage.
            public Thread executionThread;
        }
        // CodeEmbed-End: mirage1302-triggering
    }

    namespace M1302.Resolved
    {
        // CodeEmbed-Start: mirage1302-resolved
        [NetworkMessage]
        public struct StartSessionMessage
        {
            // Correct: Pass a serializable identifier instead of the raw thread object
            public string threadName;
        }
        // CodeEmbed-End: mirage1302-resolved
    }
}
