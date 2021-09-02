using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.VarIntInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        [VarInt(10, 100)]
        [SyncVar] public float value1;

        [VarInt(10, 100)]
        [SyncVar] public UnityEngine.Vector3 value2;

        [VarInt(0, 100)]
        [SyncVar] public int value3;

        [VarInt(100, 0)]
        [SyncVar] public int value4;

        [VarInt(10, 100, 0)]
        [SyncVar] public int value5;

        [VarInt(200, 100)]
        [SyncVar] public int value6;

        [VarInt(50, 100, 65)]
        [SyncVar] public int value7;

        [VarInt(50, 60)]
        [SyncVar] public int value8;

        [VarInt(500, 1000)]
        [SyncVar] public byte value9;

        [VarInt(100, 1000)]
        [SyncVar] public byte value10;

        [VarInt(50, 100, 1000)]
        [SyncVar] public byte value11;

        [BitCount(1), VarInt(50, 100, 1000)]
        [SyncVar] public int value12;

        [BitCountFromRange(-100, 100), VarInt(50, 100, 1000)]
        [SyncVar] public int value13;
    }
}