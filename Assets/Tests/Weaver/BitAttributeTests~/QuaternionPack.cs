using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace BitAttributeTests.QuaternionPack
{
    public class MyBehaviour : NetworkBehaviour
    {
        [QuaternionPack(9)]
        [SyncVar] public Quaternion value1 { get; set; }

        [QuaternionPack(10)]
        [SyncVar] public Quaternion value3 { get; set; }
    }
}