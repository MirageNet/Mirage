using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCountFromRangeInvalid
{
    // should expect 1 error per field
    public class MyBehaviour : NetworkBehaviour
    {
        // error, max must be greater than min
        [BitCountFromRange(0, 0)]
        [SyncVar] public int value1;

        // error, max must be greater than min
        [BitCountFromRange(10, 2)]
        [SyncVar] public int value2;

        // error, cant be used with zigzag
        [BitCountFromRange(0, 8), ZigZagEncode]
        [SyncVar] public int value3;

         // error, cant be used with bitcount
        [BitCount(4), BitCountFromRange(0, 8)]
        [SyncVar] public int value4;
    }
}