using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.VarIntBlocks
{
    public class MyBehaviour : NetworkBehaviour
    {
        [VarIntBlocks(4)]
        [SyncVar] public byte value1;

        [VarIntBlocks(6)]
        [SyncVar] public short value2;

        [VarIntBlocks(6)]
        [SyncVar] public ushort value3;

        [VarIntBlocks(8)]
        [SyncVar] public int value4;

        [VarIntBlocks(8)]
        [SyncVar] public uint value5;

        [VarIntBlocks(10)]
        [SyncVar] public long value6;

        [VarIntBlocks(10)]
        [SyncVar] public ulong value7;

        [VarIntBlocks(2)]
        [SyncVar] public MyByteEnum value8;

        [VarIntBlocks(3)]
        [SyncVar] public MyShortEnum value9;

        [VarIntBlocks(4)]
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