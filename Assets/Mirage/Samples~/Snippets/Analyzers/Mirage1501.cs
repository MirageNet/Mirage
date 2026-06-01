using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1501.Triggering
    {
        // CodeEmbed-Start: mirage1501-triggering
        [NetworkMessage]
        public struct HugeMessage
        {
            // Warning: Array size and primitives exceed the safe MTU threshold
            public byte[] largeBuffer; // e.g. filled with 2048 bytes of data
        }
        // CodeEmbed-End: mirage1501-triggering
    }

    namespace M1501.Resolved
    {
        // CodeEmbed-Start: mirage1501-resolved
        [NetworkMessage]
        public struct ChunkMessage
        {
            public int chunkIndex;
            // Correct: Small buffer sizes that fit comfortably within a single MTU packet
            public byte[] smallBuffer; // e.g. limited to 512 bytes per chunk
        }
        // CodeEmbed-End: mirage1501-resolved
    }
}
