using Mirage;
using System.Collections.Generic;

public class PlayerBehaviour : NetworkBehaviour
{
    [ServerRpc]
    public void CmdSendText(string {|#0:text|}) {}

    [ClientRpc]
    public void RpcUpdateList(List<int> {|#1:items|}) {}
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
