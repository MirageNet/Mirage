using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1302.Triggering
    {
        // CodeEmbed-Start: mirage1302-triggering
        [NetworkMessage]
        public class StatusMessage
        {
            public string playerName;
            
            // Non-public fields are not serialized
            private int playerHash;

            // Internal/protected fields are not serialized
            internal int internalField;
            protected float protectedField;

            // Properties are not serialized
            public int PublicProperty { get; set; }
            private string PrivateProperty { get; set; }
        }
        // CodeEmbed-End: mirage1302-triggering
    }

    namespace M1302.Resolved
    {
        // CodeEmbed-Start: mirage1302-resolved
        [NetworkMessage]
        public class StatusMessage
        {
            public string playerName;
            public int playerHash;
            public int internalField;
            public float protectedField;
        }
        // CodeEmbed-End: mirage1302-resolved
    }
}
