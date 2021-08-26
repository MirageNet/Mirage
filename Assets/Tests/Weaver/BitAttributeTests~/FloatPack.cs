using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.FloatPack
{
    public class MyBehaviour : NetworkBehaviour
    {
        [FloatPack(100f, 0.1f)]
        [SyncVar] public float value1;

        [FloatPack(1000f, 1f)]
        [SyncVar] public float value2;

        [FloatPack(10000f, 8)]
        [SyncVar] public float value3;

        [FloatPack(1f, 9)]
        [SyncVar] public float value4;
    }
}