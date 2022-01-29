namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    class CallToNonRpcBase_behaviour : CallToNonRpcBase_base
    {
        [ClientRpc]
        public override void RpcThatIsTotallyValid(int a)
        {
            // should call normal base method, no swapping to rpc (that doesn't exist)
            base.RpcThatIsTotallyValid(a);
        }
    }

    class CallToNonRpcBase_base : NetworkBehaviour
    {
        // not an rpc, override is, so it should just be called normally on receiver
        public virtual void RpcThatIsTotallyValid(int a) { }
    }

    public class CallToNonRpcBase : ClientServerSetup<MockComponent>
    {
    }
}
