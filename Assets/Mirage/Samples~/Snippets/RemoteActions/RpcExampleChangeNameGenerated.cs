using Mirage.RemoteCalls;
using Mirage.Serialization;

namespace Mirage.Snippets.RemoteActions.RpcExamplesGenerated
{
    // CodeEmbed-Start: rpc-example-change-name-generated
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public string PlayerName;

        [ServerRpc]
        public void RpcChangeName(string newName)
        {
            if (IsServer)
            {
                // direct call, skips NetworkWriter
                UserCode_RpcChangeName_123456789(newName);
            }
            else
            {
                using (var writer = NetworkWriterPool.GetWriter())
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

        protected static void Skeleton_RpcChangeName_123456789(NetworkBehaviour behaviour, NetworkReader reader, INetworkPlayer senderConnection, int replyId)
        {
            ((Player)behaviour).UserCode_RpcChangeName_123456789(reader.ReadString());
        }

        protected void RegisterRpc_1(RemoteCallCollection collection)
        {
            base.RegisterRpc(collection);
            collection.Register(0, "Mirage.Snippets.RemoteActions.RpcExamplesGenerated.Player.RpcChangeName", true, RpcInvokeType.ServerRpc, this, new RpcDelegate(Player.Skeleton_RpcChangeName_123456789), RpcRateLimitConfig.Disabled());
        }

        protected int GetRpcCount_1()
        {
            return 1;
        }
    }
    // CodeEmbed-End: rpc-example-change-name-generated
}
