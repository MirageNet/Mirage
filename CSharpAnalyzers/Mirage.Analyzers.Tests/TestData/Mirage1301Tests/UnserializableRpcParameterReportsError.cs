using Mirage;
using System.Threading;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    public void CmdStartSession(Thread {|#0:executionThread|}) {}
}
