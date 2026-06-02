namespace CustomNamespace
{
    public class ClientRpcAttribute : System.Attribute
    {
        public int target { get; set; }
    }
}

public class MyBehaviour
{
    [CustomNamespace.ClientRpc(target = 2)]
    public void RpcMessage(int connectionId)
    {
    }
}

namespace Mirage
{
    public class NetworkBehaviour {}
    
    public enum RpcTarget
    {
        Owner = 0,
        Observers = 1,
        Player = 2
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ClientRpcAttribute : System.Attribute
    {
        public RpcTarget target { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ServerRpcAttribute : System.Attribute {}

    public interface INetworkPlayer {}
    public class NetworkPlayer : INetworkPlayer {}
    public class NetworkConnection {}
}

namespace Cysharp.Threading.Tasks
{
    public struct UniTask {}
    public struct UniTask<T> {}
}
