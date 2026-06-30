using Mirage;

public class MyBehaviour<T> : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdGeneric(T value)
    {
    }

    [ServerRpc, RateLimit]
    public void CmdOther(int myValue)
    {
    }
}


public class FullClass : MyBehaviour<float>
{
    [ServerRpc, RateLimit]
    public void CmdChild(string myValue)
    {
    }
}
