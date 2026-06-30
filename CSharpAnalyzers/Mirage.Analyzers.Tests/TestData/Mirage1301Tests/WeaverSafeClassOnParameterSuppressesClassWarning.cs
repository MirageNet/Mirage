using Mirage;
using System.Threading;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdStartSession([WeaverSafeClass] Thread {|#0:executionThread|})
    {
    }
}
