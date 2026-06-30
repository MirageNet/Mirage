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
        public class UserSession
        {
            // Auto-serialized by the Weaver because it is a public field in a valid class
            public int sessionId;

            // Ignored by the Weaver during serialization to avoid errors on non-serializable utility types
            internal System.Threading.Thread workerThread;

            // Required constructor so the Weaver can instantiate this class during deserialization
            public UserSession() { }
        }

        [NetworkMessage]
        public struct StartSessionMessage
        {
            // Correct: Pass a serializable identifier instead of the raw thread object
            public string threadName;

            // Correct: Non-generic classes with parameterless constructors can be auto-serialized
            public UserSession session;
        }
        // CodeEmbed-End: mirage1301-resolved
    }
}
