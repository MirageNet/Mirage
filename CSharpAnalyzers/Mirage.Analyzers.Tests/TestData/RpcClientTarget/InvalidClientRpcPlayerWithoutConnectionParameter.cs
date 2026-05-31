using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void {|#0:RpcMessage|}(string msg)
    {
    }
}
