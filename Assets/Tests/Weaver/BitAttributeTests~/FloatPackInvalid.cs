using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.FloatPackInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        // error, unsupported type
        [FloatPack(100f, 0.1f)]
        [SyncVar] public double value1;

        // error, unsupported type
        [FloatPack(1000f, 1f)]
        [SyncVar] public int value2;

        // error, unsupported type
        [FloatPack(10000f, 8)]
        [SyncVar] public UnityEngine.Vector3 value3;

        // error, bit count out of range
        [FloatPack(1f, 31)]
        [SyncVar] public float value4;

        // error, bit count out of range
        [FloatPack(1f, 0)]
        [SyncVar] public float value5;

        // error, max can't be zero
        [FloatPack(0, 10)]
        [SyncVar] public float value6;

        // error, max can't be zero
        [FloatPack(-5, 10)]
        [SyncVar] public float value7;

        // error, precision too low
        [FloatPack(1f, float.Epsilon)]
        [SyncVar] public float value8;

        // error, precision negative
        [FloatPack(1f, -0.1f)]
        [SyncVar] public float value9;
    }
}