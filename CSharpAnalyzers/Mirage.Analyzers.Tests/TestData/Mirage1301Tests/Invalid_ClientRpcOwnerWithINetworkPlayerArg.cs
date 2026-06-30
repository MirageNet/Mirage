using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Owner)]
    public void TargetRpc(INetworkPlayer {|#0:target|}, int arg1)
    {
        //
    }
}