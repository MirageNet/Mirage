using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.BitPacking.BitCountExample1
{
    // CodeEmbed-Start: bit-count-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, BitCount(7)]
        public int Health { get; set; }
    }
    // CodeEmbed-End: bit-count-example-1
}

namespace Mirage.Snippets.BitPacking.BitCountExample2
{
    // CodeEmbed-Start: bit-count-example-2
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, BitCount(3)]
        public int WeaponIndex { get; set; }
    }
    // CodeEmbed-End: bit-count-example-2
}

namespace Mirage.Snippets.BitPacking.BitCountGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: bit-count-generated-source
        [SyncVar, BitCount(7)]
        public int myValue { get; set; }
        // CodeEmbed-End: bit-count-generated-source

        // CodeEmbed-Start: bit-count-generated-code
        public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            ulong syncVarDirtyBits = base.SyncVarDirtyBits;
            bool result = base.SerializeSyncVars(writer, initialState);

            if (initialState) 
            {
                writer.Write((ulong)this.myValue, 7);
                return true;
            }

            writer.Write(syncVarDirtyBits, 1);
            if ((syncVarDirtyBits & 1UL) != 0UL)
            {
                writer.Write((ulong)this.myValue, 7);
                result = true;
            }

            return result;
        }

        public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            base.DeserializeSyncVars(reader, initialState);

            if (initialState)
            {
                this.myValue = (int)reader.Read(7);
                return;
            }

            ulong dirtyMask = reader.Read(1);
            if ((dirtyMask & 1UL) != 0UL)
            {
                this.myValue = (int)reader.Read(7);
            }
        }
        // CodeEmbed-End: bit-count-generated-code
    }
}
