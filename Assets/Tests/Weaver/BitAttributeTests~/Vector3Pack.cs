using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace BitAttributeTests.Vector3Pack
{
    public class MyBehaviour : NetworkBehaviour
    {
        [Vector3Pack(100, 20, 80, 0.1f, 0.05f, 0.1f)]
        [SyncVar] public Vector3 value1;

        [Vector3Pack(100, 20, 80, 0.1f)]
        [SyncVar] public Vector3 value2;

        [Vector3Pack(100, 20, 80, 10, 8, 10)]
        [SyncVar] public Vector3 value3;

        [Vector3Pack(100, 20, 80, 10)]
        [SyncVar] public Vector3 value4;

        [Vector3Pack(100, 100, 100, 10)]
        [SyncVar] public Vector3 value5;
    }
}