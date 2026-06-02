using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public void RpcDoSomething(ref int {|#0:value|}) {}
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
