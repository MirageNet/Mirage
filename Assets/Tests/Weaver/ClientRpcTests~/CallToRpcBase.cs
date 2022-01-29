using Mirage;

namespace ClientRpcTests.CallToRpcBase
{
    class CallToRpcBase : BehaviourBase
    {
        [ClientRpc]
        public override void RpcThatIsTotallyValid(int a) 
        {
            // should call base user code, not generated rpc
            base.RpcThatIsTotallyValid(a);
        }
    }

    class BehaviourBase : NetworkBehaviour
    {
        [ClientRpc]
        public virtual void RpcThatIsTotallyValid(int a) { }
    }
}
