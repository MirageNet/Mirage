namespace Mirage.Tests.Runtime
{
    public class MockComponent : NetworkBehaviour
    {
        public int cmdArg1;
        public string cmdArg2;

        [ServerRpc]
        public void Send2Args(int arg1, string arg2)
        {
            cmdArg1 = arg1;
            cmdArg2 = arg2;
        }


        public INetworkPlayer cmdSender;
        [ServerRpc]
        public void SendWithSender(int arg1, INetworkPlayer sender = null)
        {
            cmdArg1 = arg1;
            cmdSender = sender;
        }

        public NetworkIdentity cmdNi;

        [ServerRpc]
        public void CmdNetworkIdentity(NetworkIdentity ni)
        {
            cmdNi = ni;
        }

        public int rpcArg1;
        public string rpcArg2;

        [ClientRpc]
        public void RpcTest(int arg1, string arg2)
        {
            rpcArg1 = arg1;
            rpcArg2 = arg2;
        }

        public int targetRpcArg1;
        public string targetRpcArg2;
        public INetworkPlayer targetRpcPlayer;

        [ClientRpc(target = Mirage.RpcTarget.Player)]
        public void ClientConnRpcTest(INetworkPlayer player, int arg1, string arg2)
        {
            targetRpcPlayer = player;
            targetRpcArg1 = arg1;
            targetRpcArg2 = arg2;
        }

        public int rpcOwnerArg1;
        public string rpcOwnerArg2;

        [ClientRpc(target = Mirage.RpcTarget.Owner)]
        public void RpcOwnerTest(int arg1, string arg2)
        {
            rpcOwnerArg1 = arg1;
            rpcOwnerArg2 = arg2;
        }
    }
}
