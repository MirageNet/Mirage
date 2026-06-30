using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Interval = 0f)]
    public void {|#0:CmdFire|}()
    {
    }
}
