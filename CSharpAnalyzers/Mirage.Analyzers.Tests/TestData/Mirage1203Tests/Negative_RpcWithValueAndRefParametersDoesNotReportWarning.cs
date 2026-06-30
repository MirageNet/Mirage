using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdDoSomething(int value, string message) {}

    [ClientRpc]
    public void RpcDoSomething(int value, string message) {}
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
