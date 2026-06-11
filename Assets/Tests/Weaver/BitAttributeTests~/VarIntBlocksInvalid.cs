using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.VarIntBlocksInValid
{
    public class MyBehaviour : NetworkBehaviour
    {
        [VarIntBlocks(0)]
        [SyncVar] public int value1 { get; set; }

        [VarIntBlocks(-2)]
        [SyncVar] public int value2 { get; set; }

        [VarIntBlocks(33)]
        [SyncVar] public int value3 { get; set; }

        [VarIntBlocks(6)]
        [SyncVar] public float value4 { get; set; }

        [VarIntBlocks(8)]
        [SyncVar] public UnityEngine.Vector3 value5 { get; set; }
    }
}