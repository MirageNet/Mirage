using System.Collections.Generic;

namespace Mirage.Tests.Runtime
{
    public class MockRpcComponent : NetworkBehaviour
    {
        public List<(int arg1, string arg2)> Server2ArgsCalls = new List<(int arg1, string arg2)>();
        public List<(int arg1, INetworkPlayer sender)> ServerWithSenderCalls = new List<(int arg1, INetworkPlayer sender)>();
        public List<NetworkIdentity> ServerWithNICalls = new List<NetworkIdentity>();
        public List<(int arg1, string arg2)> Client2ArgsCalls = new List<(int arg1, string arg2)>();
        public List<(INetworkPlayer player, int arg1, string arg2)> ClientTargetCalls = new List<(INetworkPlayer player, int arg1, string arg2)>();
        public List<(int arg1, string arg2)> ClientOwnerCalls = new List<(int arg1, string arg2)>();
        public List<(int arg1, string arg2)> ClientExcludeOwnerCalls = new List<(int arg1, string arg2)>();

        [ServerRpc]
        public void Server2Args(int arg1, string arg2)
        {
            Server2ArgsCalls.Add((arg1, arg2));
        }

        [ServerRpc]
        public void ServerWithSender(int arg1, INetworkPlayer sender = null)
        {
            ServerWithSenderCalls.Add((arg1, sender));
        }

        [ServerRpc]
        public void ServerWithNI(NetworkIdentity ni)
        {
            ServerWithNICalls.Add(ni);
        }

        [ClientRpc]
        public void Client2Args(int arg1, string arg2)
        {
            Client2ArgsCalls.Add((arg1, arg2));
        }

        [ClientRpc(target = RpcTarget.Player)]
        public void ClientTarget(INetworkPlayer player, int arg1, string arg2)
        {
            ClientTargetCalls.Add((player, arg1, arg2));
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public void ClientOwner(int arg1, string arg2)
        {
            ClientOwnerCalls.Add((arg1, arg2));
        }

        [ClientRpc(excludeOwner = true)]
        public void ClientExcludeOwner(int arg1, string arg2)
        {
            ClientExcludeOwnerCalls.Add((arg1, arg2));
        }
    }
}
