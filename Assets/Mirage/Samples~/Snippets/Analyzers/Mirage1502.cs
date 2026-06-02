using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Analyzers
{
    namespace M1502.Triggering
    {
        // CodeEmbed-Start: mirage1502-triggering
        [NetworkMessage]
        public struct ChatMessage
        {
            // Warning: Unbounded string can be exploited to send megabytes of text
            public string text;
        }
        // CodeEmbed-End: mirage1502-triggering
    }

    namespace M1502.Resolved
    {
        // CodeEmbed-Start: mirage1502-resolved
        [NetworkMessage]
        public struct ChatMessage
        {
            // Correct: Bounded string using [BitCount] to limit length
            [BitCount(8)]
            public string text;
        }
        // CodeEmbed-End: mirage1502-resolved
    }
}
