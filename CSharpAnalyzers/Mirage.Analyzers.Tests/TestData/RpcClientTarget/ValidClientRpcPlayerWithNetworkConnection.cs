using Mirage;

namespace Mirage
{
    public class NetworkConnection {}
}

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void RpcMessage(NetworkConnection conn, string msg)
    {
    }
}
