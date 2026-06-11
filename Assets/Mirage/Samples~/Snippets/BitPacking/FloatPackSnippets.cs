using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.BitPacking.FloatPackExample1
{
    // CodeEmbed-Start: float-pack-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, FloatPack(100f, 0.02f)]
        public float Health;
    }
    // CodeEmbed-End: float-pack-example-1
}

namespace Mirage.Snippets.BitPacking.FloatPackExample2
{
    // CodeEmbed-Start: float-pack-example-2
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, FloatPack(1f, 8)]
        public float Percent;
    }
    // CodeEmbed-End: float-pack-example-2
}

namespace Mirage.Snippets.BitPacking.FloatPackGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: float-pack-generated-source
        [SyncVar, FloatPack(100f, 0.02f)]
        public float myValue;
        // CodeEmbed-End: float-pack-generated-source

        // CodeEmbed-Start: float-pack-generated-code
        private FloatPacker myValue__Packer = new FloatPacker(100f, 0.02f);

        public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            ulong syncVarDirtyBits = base.SyncVarDirtyBits;
            bool result = base.SerializeSyncVars(writer, initialState);

            if (initialState) 
            {
                myValue__Packer.Pack(writer, this.myValue);
                return true;
            }

            writer.Write(syncVarDirtyBits, 1);
            if ((syncVarDirtyBits & 1UL) != 0UL)
            {
                myValue__Packer.Pack(writer, this.myValue);
                result = true;
            }

            return result;
        }

        public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            base.DeserializeSyncVars(reader, initialState);

            if (initialState)
            {
                this.myValue = myValue__Packer.Unpack(reader);
                return;
            }

            ulong dirtyMask = reader.Read(1);
            if ((dirtyMask & 1UL) != 0UL)
            {
                this.myValue = myValue__Packer.Unpack(reader);
            }
        }
        // CodeEmbed-End: float-pack-generated-code
    }
}
