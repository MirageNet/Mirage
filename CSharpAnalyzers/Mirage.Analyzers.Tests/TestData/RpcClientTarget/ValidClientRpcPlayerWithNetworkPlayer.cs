using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void RpcMessage(INetworkPlayer player, string msg)
    {
    }
}
