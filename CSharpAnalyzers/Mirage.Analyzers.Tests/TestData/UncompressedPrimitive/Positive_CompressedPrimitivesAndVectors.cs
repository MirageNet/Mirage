using Mirage;
using Mirage.Serialization;
using UnityEngine;

[NetworkMessage]
public struct ValidMessage
{
    [BitCount(8)]
    public int score;

    [FloatPack]
    public float temperature;

    [Vector2Pack]
    public Vector2 velocity;

    [Vector3Pack]
    public Vector3 position;

    [QuaternionPack]
    public Quaternion rotation;
}

namespace Mirage
{
    public class NetworkMessageAttribute : System.Attribute {}
    public class NetworkBehaviour {}
    public class SyncVarAttribute : System.Attribute {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
}
namespace Mirage.Serialization
{
    public class BitCountAttribute : System.Attribute
    {
        public BitCountAttribute(int bits) {}
    }
    public class BitCountFromRangeAttribute : System.Attribute {}
    public class VarIntAttribute : System.Attribute {}
    public class VarIntBlocksAttribute : System.Attribute {}
    public class FloatPackAttribute : System.Attribute {}
    public class Vector2PackAttribute : System.Attribute {}
    public class Vector3PackAttribute : System.Attribute {}
    public class QuaternionPackAttribute : System.Attribute {}
}
namespace UnityEngine
{
    public struct Vector2
    {
        public float x;
        public float y;
    }
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }
    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
}
