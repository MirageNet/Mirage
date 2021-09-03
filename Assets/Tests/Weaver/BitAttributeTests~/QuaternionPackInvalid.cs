using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace BitAttributeTests.QuaternionPackInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        // error, invalid type
        [QuaternionPack(9)]
        [SyncVar] public float value1;

        // error, invalid type
        [QuaternionPack(9)]
        [SyncVar] public Vector3 value2;

        // error, should be above 0
        [QuaternionPack(0)]
        [SyncVar] public Quaternion value3;

        // error, should be below 20
        [QuaternionPack(21)]
        [SyncVar] public Quaternion value4;
    }
}