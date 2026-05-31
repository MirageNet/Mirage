using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public void RpcUpdate(int value)
    {
    }
}
