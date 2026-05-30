using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace BitAttributeTests.Vector2Pack
{
    public class MyBehaviour : NetworkBehaviour
    {
        [Vector2Pack(100, 20, 0.1f, 0.1f)]
        [SyncVar] public Vector2 value1 { get; set; }

        [Vector2Pack(100, 20, 0.1f)]
        [SyncVar] public Vector2 value2 { get; set; }

        [Vector2Pack(100, 20, 10, 8)]
        [SyncVar] public Vector2 value3 { get; set; }

        [Vector2Pack(100, 20, 10)]
        [SyncVar] public Vector2 value4 { get; set; }

        [Vector2Pack(100, 100, 10)]
        [SyncVar] public Vector2 value5 { get; set; }
    }
}