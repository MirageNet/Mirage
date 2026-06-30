using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Interval = 0.5f, Refill = 5, MaxTokens = 10)]
    public void CmdFire()
    {
    }
}
