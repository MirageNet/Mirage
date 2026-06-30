using System;
using Mirage;

namespace Custom
{
    public class ServerRpcAttribute : Attribute {}
}

public class MyBehaviour : NetworkBehaviour
{
    [Custom.ServerRpc]
    public void CmdFakeRpc(ref int value) {}
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
