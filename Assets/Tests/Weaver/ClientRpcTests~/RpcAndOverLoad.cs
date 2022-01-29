using Mirage;

namespace ClientRpcTests.RpcAndOverLoad
{
    class RpcAndOverLoad : NetworkBehaviour
    {
        // normal and rpc method with same name

        [ClientRpc(target = RpcTarget.Target)]
        public void RpcThatIsTotallyValid(INetworkPlayer player, int a) { }

        public void RpcThatIsTotallyValid(int a) { }
    }
}
