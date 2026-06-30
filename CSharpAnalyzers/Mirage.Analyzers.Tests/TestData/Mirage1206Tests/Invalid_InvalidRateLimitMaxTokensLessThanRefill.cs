using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Refill = 10, MaxTokens = 5)]
    public void {|#0:CmdFire|}()
    {
    }
}
