using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdDoSomething(in int value) {}
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
