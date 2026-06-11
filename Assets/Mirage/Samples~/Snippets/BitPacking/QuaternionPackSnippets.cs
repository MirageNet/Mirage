using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.Snippets.BitPacking.QuaternionPackExample1
{
    // CodeEmbed-Start: quaternion-pack-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, QuaternionPack(9)]
        public Quaternion direction;
    }
    // CodeEmbed-End: quaternion-pack-example-1
}

namespace Mirage.Snippets.BitPacking.QuaternionPackGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: quaternion-pack-generated-source
        [SyncVar, QuaternionPack(9)]
        public Quaternion myValue;
        // CodeEmbed-End: quaternion-pack-generated-source

        // CodeEmbed-Start: quaternion-pack-generated-code
        private QuaternionPacker myValue__Packer = new QuaternionPacker(9);

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
                this.myValue = myValue__Packer.Unpack(reader);
        }
        // CodeEmbed-End: quaternion-pack-generated-code
    }
}
