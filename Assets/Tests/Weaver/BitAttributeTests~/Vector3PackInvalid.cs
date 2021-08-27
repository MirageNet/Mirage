using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace BitAttributeTests.Vector3PackInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        // error, invalid type
        [Vector3Pack(1f, 1f, 1f, 10)]
        [SyncVar] public float value1;

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 10)]
        [SyncVar] public Vector2 value2;

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 31, 31, 31)]
        [SyncVar] public Vector3 value3;

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 31)]
        [SyncVar] public Vector3 value4;

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 0, 10, 10)]
        [SyncVar] public Vector3 value5;

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 10, 0, 10)]
        [SyncVar] public Vector3 value6;

        // error, bit count out of range
        [Vector3Pack(1f, 1f, 1f, 10, 10, 0)]
        [SyncVar] public Vector3 value7;

        // error, bit count out of range
        [Vector3Pack(-1f, 1f, 1f, 10)]
        [SyncVar] public Vector3 value8;

        // error, bit count out of range
        [Vector3Pack(1f, -1f, 1f, 10)]
        [SyncVar] public Vector3 value9;

        // error, bit count out of range
        [Vector3Pack(1f, 1f, -1f, 10)]
        [SyncVar] public Vector3 value10;

        // error, bit count out of range
        [Vector3Pack(-1f, -1f, 1f, 10, 10, 10)]
        [SyncVar] public Vector3 value11;
    }
}