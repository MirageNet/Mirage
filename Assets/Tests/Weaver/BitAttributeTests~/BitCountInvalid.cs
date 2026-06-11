using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCountInvalid
{
    // should expect 1 error per field
    public class MyBehaviour : NetworkBehaviour
    {
        [BitCount(9)]
        [SyncVar] public byte value1 { get; set; }

        [BitCount(17)]
        [SyncVar] public short value2 { get; set; }

        [BitCount(17)]
        [SyncVar] public ushort value3 { get; set; }

        [BitCount(33)]
        [SyncVar] public int value4 { get; set; }

        [BitCount(33)]
        [SyncVar] public uint value5 { get; set; }

        [BitCount(65)]
        [SyncVar] public long value6 { get; set; }

        [BitCount(65)]
        [SyncVar] public ulong value7 { get; set; }

        [BitCount(9)]
        [SyncVar] public MyByteEnum value8 { get; set; }

        [BitCount(17)]
        [SyncVar] public MyShortEnum value9 { get; set; }

        [BitCount(33)]
        [SyncVar] public MyIntEnum value10 { get; set; }

        [BitCount(20)]
        [SyncVar] public UnityEngine.Vector3 value11 { get; set; }

        [BitCount(0)]
        [SyncVar] public int value12 { get; set; }

        [BitCount(-1)]
        [SyncVar] public int value13 { get; set; }
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