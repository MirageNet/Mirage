using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.FloatPack
{
    public class MyBehaviour : NetworkBehaviour
    {
        [FloatPack(100f, 0.1f)]
        [SyncVar] public float value1 { get; set; }

        [FloatPack(1000f, 1f)]
        [SyncVar] public float value2 { get; set; }

        [FloatPack(10000f, 8)]
        [SyncVar] public float value3 { get; set; }

        [FloatPack(1f, 9)]
        [SyncVar] public float value4 { get; set; }
    }
}