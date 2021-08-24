using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.ZigZag
{
    // should expect 1 error per field
    public class MyBehaviour : NetworkBehaviour
    {
        [BitCount(10), ZigZagEncode]
        [SyncVar] public int value1;

        [BitCount(4), ZigZagEncode]
        [SyncVar] public MyIntEnum value2;

        [BitCount(4), ZigZagEncode]
        [SyncVar] public MyShortEnum value3;
    }

    public enum MyIntEnum
    {
        none = 0,
        value = 1,
    }

    public enum MyShortEnum : short
    {
        none = 0,
        value = 1,
    }
}