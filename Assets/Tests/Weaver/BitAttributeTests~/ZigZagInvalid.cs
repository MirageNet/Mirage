using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.ZigZagInvalid
{
    // should expect 1 error per field
    public class MyBehaviour : NetworkBehaviour
    {
        // error, ZigZag can't be used by itself
        [ZigZagEncode]
        [SyncVar] public int value1;

        // error, ZigZag should be used on signed fields
        [BitCount(8), ZigZagEncode]
        [SyncVar] public uint value2;

        // error, ZigZag should be used on signed fields
        [BitCount(8), ZigZagEncode]
        [SyncVar] public MyShortEnum value3;
    }

    public enum MyShortEnum : ushort
    {
        none = 0,
        value = 1,
    }
}