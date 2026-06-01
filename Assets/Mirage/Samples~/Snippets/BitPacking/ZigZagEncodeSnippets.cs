using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.BitPacking.ZigZagEncodeExample1
{
    // CodeEmbed-Start: zig-zag-encode-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, BitCount(8), ZigZagEncode]
        public int modifier { get; set; }
    }
    // CodeEmbed-End: zig-zag-encode-example-1
}

namespace Mirage.Snippets.BitPacking.ZigZagEncodeGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: zig-zag-encode-generated-source
        [SyncVar, BitCount(8), ZigZagEncode]
        public int myValue { get; set; }
        // CodeEmbed-End: zig-zag-encode-generated-source

        // CodeEmbed-Start: zig-zag-encode-generated-code
        public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            ulong syncVarDirtyBits = base.SyncVarDirtyBits;
            bool result = base.SerializeSyncVars(writer, initialState);

            if (initialState) 
            {
                writer.Write((ulong)ZigZag.Encode(this.myValue), 8);
                return true;
            }

            writer.Write(syncVarDirtyBits, 1);
            if ((syncVarDirtyBits & 1UL) != 0UL)
            {
                writer.Write((ulong)ZigZag.Encode(this.myValue), 8);
                result = true;
            }

            return result;
        }

        public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            base.DeserializeSyncVars(reader, initialState);

            if (initialState)
            {
                this.myValue = (int)ZigZag.Decode(reader.Read(8));
                return;
            }

            ulong dirtyMask = reader.Read(1);
            if ((dirtyMask & 1UL) != 0UL)
                this.myValue = (int)ZigZag.Decode(reader.Read(8));
        }
        // CodeEmbed-End: zig-zag-encode-generated-code
    }
}
