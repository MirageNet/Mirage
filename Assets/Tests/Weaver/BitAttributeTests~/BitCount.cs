using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCount
{
    public class MyBehaviour : NetworkBehaviour
    {
        [BitCount(5)]
        [SyncVar] public byte value1 { get; set; }

        [BitCount(10)]
        [SyncVar] public short value2 { get; set; }

        [BitCount(9)]
        [SyncVar] public ushort value3 { get; set; }

        [BitCount(20)]
        [SyncVar] public int value4 { get; set; }

        [BitCount(10)]
        [SyncVar] public uint value5 { get; set; }

        [BitCount(48)]
        [SyncVar] public long value6 { get; set; }

        [BitCount(5)]
        [SyncVar] public ulong value7 { get; set; }

        [BitCount(3)]
        [SyncVar] public MyByteEnum value8 { get; set; }

        [BitCount(10)]
        [SyncVar] public MyShortEnum value9 { get; set; }

        [BitCount(5)]
        [SyncVar] public MyIntEnum value10 { get; set; }
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