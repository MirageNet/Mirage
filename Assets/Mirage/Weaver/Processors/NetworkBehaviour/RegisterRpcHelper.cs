using Mirage.CodeGen;
using Mirage.RemoteCalls;
using Mono.Cecil;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class RegisterRpcHelper : BaseMethodHelper
    {
        public RegisterRpcHelper(ModuleDefinition module, TypeDefinition typeDefinition) : base(module, typeDefinition)
        {
        }

        public override string MethodName => nameof(NetworkBehaviour.RegisterRpc);

        protected override void AddParameters()
        {
            Method.AddParam<RemoteCallCollection>("collection");
        }

        protected override void AddLocals()
        {
        }
    }
}
