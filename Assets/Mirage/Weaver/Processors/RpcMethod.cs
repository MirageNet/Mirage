using Mono.Cecil;

namespace Mirage.Weaver
{
    public abstract class RpcMethod
    {
        public MethodDefinition stub;
        public MethodDefinition skeleton;
    }

    public class ServerRpcMethod : RpcMethod
    {
        public bool requireAuthority;
    }

    public class ClientRpcMethod : RpcMethod
    {
        public RpcTarget target;
        public bool excludeOwner;
    }
}
