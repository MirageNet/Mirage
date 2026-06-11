using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.VarIntInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        [VarInt(10, 100)]
        [SyncVar] public float value1 { get; set; }

        [VarInt(10, 100)]
        [SyncVar] public UnityEngine.Vector3 value2 { get; set; }

        [VarInt(0, 100)]
        [SyncVar] public int value3 { get; set; }

        [VarInt(100, 0)]
        [SyncVar] public int value4 { get; set; }

        [VarInt(10, 100, 0)]
        [SyncVar] public int value5 { get; set; }

        [VarInt(200, 100)]
        [SyncVar] public int value6 { get; set; }

        [VarInt(50, 100, 65)]
        [SyncVar] public int value7 { get; set; }

        [VarInt(50, 60)]
        [SyncVar] public int value8 { get; set; }

        [VarInt(500, 1000)]
        [SyncVar] public byte value9 { get; set; }

        [VarInt(100, 1000)]
        [SyncVar] public byte value10 { get; set; }

        [VarInt(50, 100, 1000)]
        [SyncVar] public byte value11 { get; set; }

        [BitCount(1), VarInt(50, 100, 1000)]
        [SyncVar] public int value12 { get; set; }

        [BitCountFromRange(-100, 100), VarInt(50, 100, 1000)]
        [SyncVar] public int value13 { get; set; }
    }
}