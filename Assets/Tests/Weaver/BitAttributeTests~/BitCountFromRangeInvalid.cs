using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCountFromRangeInvalid
{
    // should expect 1 error per field
    public class MyBehaviour : NetworkBehaviour
    {
        // error, max must be greater than min
        [BitCountFromRange(0, 0)]
        [SyncVar] public int value1 { get; set; }

        // error, max must be greater than min
        [BitCountFromRange(10, 2)]
        [SyncVar] public int value2 { get; set; }

        // error, cant be used with bitcount
        [BitCount(4), BitCountFromRange(0, 8)]
        [SyncVar] public int value3 { get; set; }

        // error, max is above type max
        [BitCountFromRange(0, 300)]
        [SyncVar] public byte value4 { get; set; }

        // error, max is above type max
        [BitCountFromRange(0, int.MaxValue)]
        [SyncVar] public short value5 { get; set; }

        // error, min is below type max
        [BitCountFromRange(-50, 50)]
        [SyncVar] public uint value6 { get; set; }

        // error, unsupported type
        [BitCountFromRange(-50, 50)]
        [SyncVar] public UnityEngine.Vector3 value7 { get; set; }

        // error, unsupported type
        [BitCountFromRange(-50, 50)]
        [SyncVar] public long value8 { get; set; }
    }
}