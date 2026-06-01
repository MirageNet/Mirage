using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.BitPacking.VarIntBlocksExample1
{
    // CodeEmbed-Start: var-int-blocks-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, VarIntBlocks(6)]
        public int modifier { get; set; }
    }
    // CodeEmbed-End: var-int-blocks-example-1
}

namespace Mirage.Snippets.BitPacking.VarIntBlocksExample2
{
    // CodeEmbed-Start: var-int-blocks-example-2
    public enum MyDirection
    {
        Backwards = -1,
        None = 0,
        Forwards = 1,
    }

    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, VarIntBlocks(2)]
        public MyDirection direction { get; set; }
    }
    // CodeEmbed-End: var-int-blocks-example-2
}

namespace Mirage.Snippets.BitPacking.VarIntBlocksGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: var-int-blocks-generated-source
        [SyncVar, VarIntBlocks(6)]
        public int myValue { get; set; }
        // CodeEmbed-End: var-int-blocks-generated-source

        // CodeEmbed-Start: var-int-blocks-generated-code
        public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            ulong syncVarDirtyBits = base.SyncVarDirtyBits;
            bool result = base.SerializeSyncVars(writer, initialState);

            if (initialState) 
            {
                VarIntBlocksPacker.Pack(writer, (ulong)this.myValue, 6);
                return true;
            }

            writer.Write(syncVarDirtyBits, 1);
            if ((syncVarDirtyBits & 1UL) != 0UL)
            {
                VarIntBlocksPacker.Pack(writer, (ulong)this.myValue, 6);
                result = true;
            }

            return result;
        }

        public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            base.DeserializeSyncVars(reader, initialState);

            if (initialState)
            {
                this.myValue = (int)VarIntBlocksPacker.Unpack(reader, 6);
                return;
            }

            ulong dirtyMask = reader.Read(1);
            if ((dirtyMask & 1UL) != 0UL)
                this.myValue = (int)VarIntBlocksPacker.Unpack(reader, 6);
        }
        // CodeEmbed-End: var-int-blocks-generated-code
    }
}
