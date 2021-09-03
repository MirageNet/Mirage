using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCountInvalid
{
    // should expect 1 error per field
    public class MyBehaviour : NetworkBehaviour
    {
        [BitCount(9)]
        [SyncVar] public byte value1;

        [BitCount(17)]
        [SyncVar] public short value2;

        [BitCount(17)]
        [SyncVar] public ushort value3;

        [BitCount(33)]
        [SyncVar] public int value4;

        [BitCount(33)]
        [SyncVar] public uint value5;

        [BitCount(65)]
        [SyncVar] public long value6;

        [BitCount(65)]
        [SyncVar] public ulong value7;

        [BitCount(9)]
        [SyncVar] public MyByteEnum value8;

        [BitCount(17)]
        [SyncVar] public MyShortEnum value9;

        [BitCount(33)]
        [SyncVar] public MyIntEnum value10;

        [BitCount(20)]
        [SyncVar] public UnityEngine.Vector3 value11;

        [BitCount(0)]
        [SyncVar] public int value12;

        [BitCount(-1)]
        [SyncVar] public int value13;
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