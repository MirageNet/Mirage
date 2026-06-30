using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    public void CmdWithSender(int arg1, INetworkPlayer sender = null)
    {
        //
    }
}