using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void TargetRpc(INetworkPlayer target, INetworkPlayer {|#0:target2|}, int arg1)
    {
        //
    }
}