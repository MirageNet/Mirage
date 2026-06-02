using System;
using Mirage;
using Mirage.Serialization;
using Mirage.RemoteCalls;

namespace Mirage.Snippets.RemoteActions.RpcExamplesGenerated
{
    using NetworkBehaviour = DummyNetworkBehaviour;
    // Dummy classes/aliases to make the snippet code compile in Unity
    public class DummyNetworkBehaviour
    {
        public bool IsServer { get; set; }
        public DummyRemoteCallCollection remoteCallCollection { get; set; } = new DummyRemoteCallCollection();
        protected virtual int GetRpcCount() => 0;
    }

    public class DummyRemoteCallCollection
    {
        public void Register(int id, Type type, string name, RpcInvokeType invokeType, CmdDelegate cmdDelegate, bool requireAuthority) {}
    }

    public delegate void CmdDelegate(NetworkReader reader, INetworkPlayer senderConnection, int replyId);

    public static class ServerRpcSender
    {
        public static void Send(DummyNetworkBehaviour behaviour, int index, PooledNetworkWriter writer, int channel, bool requireAuthority) {}
    }

    // CodeEmbed-Start: rpc-example-change-name-generated
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public string PlayerName { get; set; }

        [ServerRpc]
        public void RpcChangeName(string newName)
        {
            if (this.IsServer)
                UserCode_RpcChangeName_123456789(newName);
            else
            {
                using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
                {
                    writer.WriteString(newName);
                    ServerRpcSender.Send(this, 123456789, writer, 0, true);
                }
            }
        }

        public void UserCode_RpcChangeName_123456789(string newName)
        {
            PlayerName = newName;
        }

        protected void Skeleton_RpcChangeName_123456789(NetworkReader reader, INetworkPlayer senderConnection, int replyId)
        {
            this.UserCode_RpcChangeName_123456789(reader.ReadString());
        }

        public Player()
        {
            this.remoteCallCollection.Register(0, typeof(Player), "Player.RpcChangeName", RpcInvokeType.ServerRpc, new CmdDelegate(Skeleton_RpcChangeName_123456789), true);
        }

        protected override int GetRpcCount()
        {
            return 1;
        }
    }
    // CodeEmbed-End: rpc-example-change-name-generated
}
