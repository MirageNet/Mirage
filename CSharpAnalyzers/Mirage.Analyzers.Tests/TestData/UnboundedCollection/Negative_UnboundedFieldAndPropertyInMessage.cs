using Mirage;
using System.Collections.Generic;

[NetworkMessage]
public struct InvalidMessage
{
    public string {|#0:Name|};

    public int[] {|#1:Scores|};

    public List<float> {|#2:Positions|} { get; set; }
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
