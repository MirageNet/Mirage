using Mirage;

namespace ClientRpcTests.CallToNonRpcOverLoadReverse
{
    class CallToNonRpcOverLoadReverse : NetworkBehaviour
    {
        // reverse define order to CallToNonRpcOverLoad
        public void RpcThatIsTotallyValid(int a) { }

        [ClientRpc(target = RpcTarget.Player)]
        public void RpcThatIsTotallyValid(INetworkPlayer player, int a) 
        { 
            // should call overload without any problem
            RpcThatIsTotallyValid(a);
        }
    }
}
