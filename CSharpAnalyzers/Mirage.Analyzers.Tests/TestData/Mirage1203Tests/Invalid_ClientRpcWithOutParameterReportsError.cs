using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public void RpcDoSomething(out int {|#0:value|})
    {
        value = 0;
    }
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
