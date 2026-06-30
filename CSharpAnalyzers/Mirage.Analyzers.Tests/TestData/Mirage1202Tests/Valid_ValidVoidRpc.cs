using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdReturnsVoid()
    {
    }
}
