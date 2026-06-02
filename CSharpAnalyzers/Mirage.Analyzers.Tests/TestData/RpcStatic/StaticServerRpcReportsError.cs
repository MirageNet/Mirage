using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public static void {|#0:CmdDoSomething|}() {}
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
