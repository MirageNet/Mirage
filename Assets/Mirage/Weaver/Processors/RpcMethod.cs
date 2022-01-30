using Mono.Cecil;

namespace Mirage.Weaver
{
    public abstract class RpcMethod
    {
        /// <summary>Original method created by user, body replaced with code that serializes params and sends message</summary>
        public MethodDefinition stub;
        /// <summary>Method that receives the call and deserialize parmas</summary>
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
