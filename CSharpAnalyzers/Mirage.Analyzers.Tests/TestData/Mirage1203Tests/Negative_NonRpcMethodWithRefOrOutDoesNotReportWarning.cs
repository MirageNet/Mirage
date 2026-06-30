using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    public void LocalHelper(ref int value, out string result)
    {
        value += 1;
        result = "done";
    }
}

namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
