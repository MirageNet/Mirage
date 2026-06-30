using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Interval = 1f, Refill = 10, MaxTokens = 10)]
    public void CmdInteract()
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
