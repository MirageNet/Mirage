using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.FloatPacker
{
    public class MyBehaviour : NetworkBehaviour
    {
        [FloatPacker(100f, 0.1f)]
        [SyncVar] public float value1;

        [FloatPacker(1000f, 1f)]
        [SyncVar] public float value2;

        [FloatPacker(10000f, 8)]
        [SyncVar] public float value3;

        [FloatPacker(1f, 9)]
        [SyncVar] public float value4;
    }
}