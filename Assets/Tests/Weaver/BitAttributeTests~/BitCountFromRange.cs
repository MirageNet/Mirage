using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCountFromRange
{
    // should expect 1 error per field
    public class MyBehaviour : NetworkBehaviour
    {
        [BitCountFromRange(-100, 100)]
        [SyncVar] public int value1;

        [BitCountFromRange(100, 1000)]
        [SyncVar] public int value2;

        [BitCountFromRange(0, 1000)]
        [SyncVar] public uint value3;

        [BitCountFromRange(0, 250)]
        [SyncVar] public byte value4;

        [BitCountFromRange(-1, 1)]
        [SyncVar] public MyDirection value5;

        [BitCountFromRange(0, 1)]
        [SyncVar] public MyShortEnum value6;

        [BitCountFromRange(0, 6)]
        [SyncVar] public System.DayOfWeek value7;
    }

    public enum MyDirection  {
        left = -1,
        none = 0,
        right = 1
    }

    public enum MyShortEnum : ushort
    {
        none = 0,
        value = 1,
    }
}