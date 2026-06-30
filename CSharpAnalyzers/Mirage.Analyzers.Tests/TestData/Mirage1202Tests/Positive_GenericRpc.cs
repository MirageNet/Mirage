using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void {|#0:CmdGeneric|}<T>(T val)
    {
    }
}
