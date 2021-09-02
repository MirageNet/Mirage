using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.VarIntBlocksInValid
{
    public class MyBehaviour : NetworkBehaviour
    {
        [VarIntBlocks(0)]
        [SyncVar] public int value1;

        [VarIntBlocks(-2)]
        [SyncVar] public int value2;

        [VarIntBlocks(33)]
        [SyncVar] public int value3;

        [VarIntBlocks(6)]
        [SyncVar] public float value4;

        [VarIntBlocks(8)]
        [SyncVar] public UnityEngine.Vector3 value5;
    }
}