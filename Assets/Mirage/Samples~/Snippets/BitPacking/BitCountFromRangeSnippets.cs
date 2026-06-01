using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.BitPacking.BitCountFromRangeExample1
{
    // CodeEmbed-Start: bit-count-from-range-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, BitCountFromRange(-100, 100)]
        public int modifier { get; set; }
    }
    // CodeEmbed-End: bit-count-from-range-example-1
}

namespace Mirage.Snippets.BitPacking.BitCountFromRangeExample2
{
    internal class BitCountAttribute : System.Attribute
    {
        public BitCountAttribute(int min, int max) {}
    }

    // CodeEmbed-Start: bit-count-from-range-example-2
    public enum MyDirection
    {
        Backwards = -1,
        None = 0,
        Forwards = 1,
    }
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, BitCount(-1, 1)]
        public MyDirection direction { get; set; }
    }
    // CodeEmbed-End: bit-count-from-range-example-2
}

namespace Mirage.Snippets.BitPacking.BitCountFromRangeGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: bit-count-from-range-generated-source
        [SyncVar, BitCountFromRange(-100, 100)]
        public int myValue { get; set; }
        // CodeEmbed-End: bit-count-from-range-generated-source

        // CodeEmbed-Start: bit-count-from-range-generated-code
        public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            ulong syncVarDirtyBits = base.SyncVarDirtyBits;
            bool result = base.SerializeSyncVars(writer, initialState);

            if (initialState) 
            {
                writer.Write((ulong)(this.myValue - (-100)), 8);
                return true;
            }

            writer.Write(syncVarDirtyBits, 1);
            if ((syncVarDirtyBits & 1UL) != 0UL)
            {
                writer.Write((ulong)(this.myValue - (-100)), 8);
                result = true;
            }

            return result;
        }

        public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            base.DeserializeSyncVars(reader, initialState);

            if (initialState)
            {
                this.myValue = (int)reader.Read(8) + (-100);
                return;
            }
            
            ulong dirtyMask = reader.Read(1);
            if ((dirtyMask & 1UL) != 0UL)
            {
                this.myValue = (int)reader.Read(8) + (-100);
            }
        }
        // CodeEmbed-End: bit-count-from-range-generated-code
    }
}
