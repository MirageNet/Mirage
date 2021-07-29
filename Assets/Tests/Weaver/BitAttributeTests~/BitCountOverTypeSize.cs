using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.BitCountOverTypeSize
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
    }
}