using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Observers)]
    public void TargetRpc(INetworkPlayer {|#0:target|}, int arg1)
    {
        //
    }
}