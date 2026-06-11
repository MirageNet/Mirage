using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.Snippets.BitPacking.VectorPackExample1
{
    // CodeEmbed-Start: vector-pack-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, Vector3Pack(100f, 100f, 100f, 0.05f)]
        public Vector3 Position;
    }
    // CodeEmbed-End: vector-pack-example-1
}

namespace Mirage.Snippets.BitPacking.VectorPackExample2
{
    // CodeEmbed-Start: vector-pack-example-2
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, Vector3Pack(100f, 20f, 100f, 0.05f, 0.1f, 0.05f)]
        public Vector3 Position;
    }
    // CodeEmbed-End: vector-pack-example-2
}

namespace Mirage.Snippets.BitPacking.VectorPackExample3
{
    // CodeEmbed-Start: vector-pack-example-3
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, Vector2Pack(1000f, 80f, 0.05f)]
        public Vector2 Position;
    }
    // CodeEmbed-End: vector-pack-example-3
}

namespace Mirage.Snippets.BitPacking.VectorPackGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: vector-pack-generated-source
        [SyncVar, Vector3Pack(100f, 20f, 100f, 0.05f, 0.1f, 0.05f)]
        public Vector3 myValue1;

        [SyncVar, Vector2Pack(1000f, 80f, 0.05f)]
        public Vector2 myValue2;
        // CodeEmbed-End: vector-pack-generated-source

        // CodeEmbed-Start: vector-pack-generated-code
        private Vector3Packer myValue1__Packer = new Vector3Packer(100f, 20f, 100f, 0.05f, 0.1f, 0.05f);
        private Vector2Packer myValue2__Packer = new Vector2Packer(1000f, 80f, 0.05f, 0.05f);

        public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            ulong syncVarDirtyBits = base.SyncVarDirtyBits;
            bool result = base.SerializeSyncVars(writer, initialState);

            if (initialState) 
            {
                myValue1__Packer.Pack(writer, this.myValue1);
                myValue2__Packer.Pack(writer, this.myValue2);
                return true;
            }

            writer.Write(syncVarDirtyBits, 2);
            if ((syncVarDirtyBits & 1UL) != 0UL)
            {
                myValue1__Packer.Pack(writer, this.myValue1);
                result = true;
            }
            if ((syncVarDirtyBits & 2UL) != 0UL)
            {
                myValue2__Packer.Pack(writer, this.myValue2);
                result = true;
            }

            return result;
        }

        public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            base.DeserializeSyncVars(reader, initialState);

            if (initialState)
            {
                this.myValue1 = myValue1__Packer.Unpack(reader);
                this.myValue2 = myValue2__Packer.Unpack(reader);
                return;
            }

            ulong dirtyMask = reader.Read(2);
            if ((dirtyMask & 1UL) != 0UL)
                this.myValue1 = myValue1__Packer.Unpack(reader);
            if ((dirtyMask & 2UL) != 0UL)
                this.myValue2 = myValue2__Packer.Unpack(reader);
        }
        // CodeEmbed-End: vector-pack-generated-code
    }
}
