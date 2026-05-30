using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.VarIntBlocks
{
    public class MyBehaviour : NetworkBehaviour
    {
        [VarIntBlocks(4)]
        [SyncVar] public byte value1 { get; set; }

        [VarIntBlocks(6)]
        [SyncVar] public short value2 { get; set; }

        [VarIntBlocks(6)]
        [SyncVar] public ushort value3 { get; set; }

        [VarIntBlocks(8)]
        [SyncVar] public int value4 { get; set; }

        [VarIntBlocks(8)]
        [SyncVar] public uint value5 { get; set; }

        [VarIntBlocks(10)]
        [SyncVar] public long value6 { get; set; }

        [VarIntBlocks(10)]
        [SyncVar] public ulong value7 { get; set; }

        [VarIntBlocks(2)]
        [SyncVar] public MyByteEnum value8 { get; set; }

        [VarIntBlocks(3)]
        [SyncVar] public MyShortEnum value9 { get; set; }

        [VarIntBlocks(4)]
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