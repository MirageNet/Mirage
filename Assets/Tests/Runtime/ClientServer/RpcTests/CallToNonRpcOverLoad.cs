namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    class CallToNonRpcOverLoad_behaviour : NetworkBehaviour
    {
        // normal and rpc method in same class

        [ClientRpc(target = RpcTarget.Player)]
        public void RpcThatIsTotallyValid(INetworkPlayer player, int a)
        {
            // should call overload without any problem
            RpcThatIsTotallyValid(a);
        }

        public void RpcThatIsTotallyValid(int a) { }
    }

    public class CallToNonRpcOverLoad : ClientServerSetup<MockComponent>
    {
    }
}
