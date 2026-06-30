using Mirage;

namespace CustomNamespace
{
    public class RateLimitAttribute : System.Attribute {}
}

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [CustomNamespace.RateLimit]
    public void {|#0:CmdInteract|}()
    {
    }
}

namespace Mirage
{
    public class NetworkBehaviour {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ServerRpcAttribute : System.Attribute {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ClientRpcAttribute : System.Attribute {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class RateLimitAttribute : System.Attribute
    {
        public float Interval { get; set; }
        public int Refill { get; set; }
        public int MaxTokens { get; set; }
    }
}
