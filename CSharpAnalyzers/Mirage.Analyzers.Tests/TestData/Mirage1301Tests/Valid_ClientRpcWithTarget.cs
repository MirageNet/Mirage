using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void TargetRpc(INetworkPlayer target, int arg1)
    {
        //
    }
}