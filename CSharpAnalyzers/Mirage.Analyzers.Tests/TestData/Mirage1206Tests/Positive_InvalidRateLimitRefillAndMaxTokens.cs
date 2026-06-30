using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Refill = -5, MaxTokens = 0)]
    public void {|#0:CmdFire|}()
    {
    }
}
