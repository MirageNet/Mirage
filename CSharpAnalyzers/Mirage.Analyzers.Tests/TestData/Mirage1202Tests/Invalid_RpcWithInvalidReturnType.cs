using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public int {|#0:CmdReturnsInt|}()
    {
        return 0;
    }
}
