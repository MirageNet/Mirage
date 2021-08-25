using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCount
{
    public class MyBehaviour : NetworkBehaviour
    {
        [BitCount(5)]
        [SyncVar] public byte value1;

        [BitCount(10)]
        [SyncVar] public short value2;

        [BitCount(9)]
        [SyncVar] public ushort value3;

        [BitCount(20)]
        [SyncVar] public int value4;

        [BitCount(10)]
        [SyncVar] public uint value5;

        [BitCount(48)]
        [SyncVar] public long value6;

        [BitCount(5)]
        [SyncVar] public ulong value7;

        [BitCount(3)]
        [SyncVar] public MyByteEnum value8;

        [BitCount(10)]
        [SyncVar] public MyShortEnum value9;

        [BitCount(5)]
        [SyncVar] public MyIntEnum value10;
    }

    public enum MyIntEnum
    {
        none = 0,
        value = 1,
    }

    public enum MyShortEnum : ushort
    {
        none = 0,
        value = 1,
    }

    public enum MyByteEnum : byte
    {
        none = 0,
        value = 1,
    }
}