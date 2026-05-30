using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace BitAttributeTests.QuaternionPackInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        // error, invalid type
        [QuaternionPack(9)]
        [SyncVar] public float value1 { get; set; }

        // error, invalid type
        [QuaternionPack(9)]
        [SyncVar] public Vector3 value2 { get; set; }

        // error, should be above 0
        [QuaternionPack(0)]
        [SyncVar] public Quaternion value3 { get; set; }

        // error, should be below 20
        [QuaternionPack(21)]
        [SyncVar] public Quaternion value4 { get; set; }
    }
}