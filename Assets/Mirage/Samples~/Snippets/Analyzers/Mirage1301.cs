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
            // Error: Thread is not serializable
            public Thread executionThread;
        }
        // CodeEmbed-End: mirage1301-triggering
    }

    namespace M1301.Resolved
    {
        // CodeEmbed-Start: mirage1301-resolved
        public class UserSession
        {
            // Auto-serialized: public field in a valid class
            public int sessionId;

            // Ignored during serialization
            [System.NonSerialized] public System.Threading.Thread workerThread;

            // Public parameterless constructor for generated deserialization
            public UserSession() { }
        }

        [NetworkMessage]
        public struct StartSessionMessage
        {
            // Use a serializable identifier instead of the thread object
            public string threadName;

            // Auto-serialized: concrete class with a public parameterless constructor
            public UserSession session;
        }
        // CodeEmbed-End: mirage1301-resolved
    }
}
