using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace BitAttributeTests.Vector3PackInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        // error, invalid type
        [Vector3Pack(1f, 1f, 1f, 10)]
        [SyncVar] public float value1 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 10)]
        [SyncVar] public Vector2 value2 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 31, 31, 31)]
        [SyncVar] public Vector3 value3 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 31)]
        [SyncVar] public Vector3 value4 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 0, 10, 10)]
        [SyncVar] public Vector3 value5 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 10, 0, 10)]
        [SyncVar] public Vector3 value6 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 10, 10, 0)]
        [SyncVar] public Vector3 value7 { get; set; }

        // error, bit count out of range
        [Vector3Pack(-1f, 1f, 1f, 10)]
        [SyncVar] public Vector3 value8 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, -1f, 1f, 10)]
        [SyncVar] public Vector3 value9 { get; set; }

        // error, bit count out of range
        [Vector3Pack(1f, 1f, -1f, 10)]
        [SyncVar] public Vector3 value10 { get; set; }

        // error, bit count out of range
        [Vector3Pack(-1f, -1f, 1f, 10, 10, 10)]
        [SyncVar] public Vector3 value11 { get; set; }
    }
}