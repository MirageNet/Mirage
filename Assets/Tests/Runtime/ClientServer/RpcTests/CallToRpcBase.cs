namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    class CallToRpcBase_behaviour : CallToRpcBase_base
    {
        [ClientRpc]
        public override void RpcThatIsTotallyValid(int a)
        {
            // should call base user code, not generated rpc
            base.RpcThatIsTotallyValid(a);
        }
    }

    class CallToRpcBase_base : NetworkBehaviour
    {
        [ClientRpc]
        public virtual void RpcThatIsTotallyValid(int a) { }
    }

    public class CallToRpcBase : ClientServerSetup<MockComponent>
    {
    }
}
