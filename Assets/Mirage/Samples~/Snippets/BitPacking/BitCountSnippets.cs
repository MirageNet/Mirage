using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.BitPacking.BitCountExample1
{
    // CodeEmbed-Start: bit-count-example-1
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, BitCount(7)]
        public int Health;
    }
    // CodeEmbed-End: bit-count-example-1
}

namespace Mirage.Snippets.BitPacking.BitCountExample2
{
    // CodeEmbed-Start: bit-count-example-2
    public class MyNetworkBehaviour : NetworkBehaviour 
    {
        [SyncVar, BitCount(3)]
        public int WeaponIndex;
    }
    // CodeEmbed-End: bit-count-example-2
}

namespace Mirage.Snippets.BitPacking.BitCountEnumExample
{
    // CodeEmbed-Start: bit-count-enum-example
    [BitCount(2)]
    public enum WeaponType
    {
        Pistol = 0,
        Rifle = 1,
        Shotgun = 2,
        Rocket = 3,
    }

    public class MyNetworkBehaviour : NetworkBehaviour
    {
        // sent using 2 bits, from the attribute on WeaponType
        [SyncVar]
        public WeaponType PrimaryWeapon;

        // attribute on field overrides the one on WeaponType
        [SyncVar, BitCount(4)]
        public WeaponType SecondaryWeapon;

        // each element in the list is sent using 2 bits
        [ClientRpc]
        public void RpcSetLoadout(System.Collections.Generic.List<WeaponType> weapons) { }
    }
    // CodeEmbed-End: bit-count-enum-example
}

namespace Mirage.Snippets.BitPacking.BitCountGenerated
{
    public class GeneratedExample : NetworkBehaviour
    {
        // CodeEmbed-Start: bit-count-generated-source
        [SyncVar, BitCount(7)]
        public int myValue;
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
