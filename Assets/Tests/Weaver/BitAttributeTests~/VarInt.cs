using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.VarInt
{
    public class MyBehaviour : NetworkBehaviour
    {
        [VarInt(10, 100)]
        [SyncVar] public byte value1;

        [VarInt(10, 500)]
        [SyncVar] public short value2;

        [VarInt(10, 500)]
        [SyncVar] public ushort value3;

        [VarInt(10, 500)]
        [SyncVar] public int value4;

        [VarInt(10, 500, 10000)]
        [SyncVar] public uint value5;

        [VarInt(10, 500, 10000, false)]
        [SyncVar] public long value6;

        [VarInt(10, 500, 10000, false)]
        [SyncVar] public ulong value7;

        [VarInt(3, 10, 60)]
        [SyncVar] public MyByteEnum value8;

        [VarInt(3, 10, 60)]
        [SyncVar] public MyShortEnum value9;

        [VarInt(3, 10, 60)]
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
