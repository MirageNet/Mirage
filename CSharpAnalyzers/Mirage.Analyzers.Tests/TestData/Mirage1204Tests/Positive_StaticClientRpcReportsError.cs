using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public static void {|#0:RpcDoSomething|}() {}
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
