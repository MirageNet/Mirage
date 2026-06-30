using Mirage;

namespace CustomNamespace
{
    public class RateLimitAttribute : System.Attribute
    {
        public float Interval { get; set; }
        public int Refill { get; set; }
        public int MaxTokens { get; set; }
    }
}

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [CustomNamespace.RateLimit(Interval = -1f)]
    public void {|#0:CmdFire|}()
    {
    }
}
