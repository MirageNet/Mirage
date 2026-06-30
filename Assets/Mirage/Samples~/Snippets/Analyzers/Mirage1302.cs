using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1302.Triggering
    {
        // CodeEmbed-Start: mirage1302-triggering
        [NetworkMessage]
        public struct StatusMessage
        {
            public string playerName;
            
            // Warning: Private fields are not serialized by the Weaver
            private int playerHash;
        }
        // CodeEmbed-End: mirage1302-triggering
    }

    namespace M1302.Resolved
    {
        // CodeEmbed-Start: mirage1302-resolved
        [NetworkMessage]
        public struct StatusMessage
        {
            public string playerName;
            public int playerHash;
        }
        // CodeEmbed-End: mirage1302-resolved
    }
}
