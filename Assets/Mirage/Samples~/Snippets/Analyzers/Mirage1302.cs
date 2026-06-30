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
            
            // Warning: Private fields are not serialized by the Weaver
            private int playerHash;

            // Warning: Internal and protected fields are also ignored by the Weaver
            internal int internalField;
            protected float protectedField;

            // Warning: Properties (including public ones) are ignored by the Weaver
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
