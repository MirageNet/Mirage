using Mirage;
using Mirage.Serialization;
using System.Collections.Generic;

[NetworkMessage]
public struct ValidMessage
{
    [BitCount(8)]
    public string Name;

    [VarInt]
    public int[] Scores;

    [BitCount(10)]
    public List<float> Positions;
}

namespace Mirage
{
    public class NetworkMessageAttribute : System.Attribute {}
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
}
namespace Mirage.Serialization
{
    public class BitCountAttribute : System.Attribute
    {
        public BitCountAttribute(int bits) {}
    }
    public class VarIntAttribute : System.Attribute {}
    public class BitCountFromRangeAttribute : System.Attribute {}
    public class VarIntBlocksAttribute : System.Attribute {}
}
