using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.FloatPackerInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        // error, unsupported type
        [FloatPacker(100f, 0.1f)]
        [SyncVar] public double value1;

        // error, unsupported type
        [FloatPacker(1000f, 1f)]
        [SyncVar] public int value2;

        // error, unsupported type
        [FloatPacker(10000f, 8)]
        [SyncVar] public UnityEngine.Vector3 value3;

        // error, bit count out of range
        [FloatPacker(1f, 31)]
        [SyncVar] public float value4;

        // error, bit count out of range
        [FloatPacker(1f, 0)]
        [SyncVar] public float value5;

        // error, max can't be zero
        [FloatPacker(0, 10)]
        [SyncVar] public float value6;

        // error, max can't be zero
        [FloatPacker(-5, 10)]
        [SyncVar] public float value7;

        // error, precision too low
        [FloatPacker(1f, float.Epsilon)]
        [SyncVar] public float value8;

        // error, precision negative
        [FloatPacker(1f, -0.1f)]
        [SyncVar] public float value9;
    }
}