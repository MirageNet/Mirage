using Mirage;

namespace ClientRpcTests.ClientRpcOverload
{
    public class ClientRpcOverload : NetworkBehaviour
    {
        [ServerRpc(requireAuthority = false)]
        public void MyRpc(int arg1, INetworkPlayer sender)
        {

        }
        [ServerRpc(requireAuthority = false)]
        public void MyRpc(int arg1, bool option1, INetworkPlayer sender)
        {

        }

        [ClientRpc]
        public void MyRpc(int arg1)
        {

        }
        [ClientRpc]
        public void MyRpc(int arg1, bool option1)
        {

        }
    }
}
