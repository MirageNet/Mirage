using Mirage;

namespace ClientRpcTests.CallToNonRpcBase
{
    class CallToNonRpcBase : BehaviourBase
    {
        [ClientRpc]
        public override void RpcThatIsTotallyValid(int a) 
        { 
            // should call normal base method, no swapping to rpc (that doesn't exist)
            base.RpcThatIsTotallyValid(a);
        }
    }

    class BehaviourBase : NetworkBehaviour
    {
        // not an rpc, override is, so it should just be called normally on receiver
        public virtual void RpcThatIsTotallyValid(int a) { }
    }
}
