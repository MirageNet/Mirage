using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;

namespace Mirage.RemoteCalls
{
    /// <summary>
    /// Methods used by weaver to send RPCs
    /// </summary>
    public static class ServerRpcSender
    {
        public static void Send(NetworkBehaviour behaviour, int hash, NetworkWriter writer, int channelId, bool requireAuthority)
        {
            ServerRpcMessage message = CreateMessage(behaviour, hash, writer, requireAuthority);

            behaviour.Client.Send(message, channelId);
        }

        public static UniTask<T> SendWithReturn<T>(NetworkBehaviour behaviour, int hash, NetworkWriter writer, int channelId, bool requireAuthority)
        {
            ServerRpcMessage message = CreateMessage(behaviour, hash, writer, requireAuthority);

            (UniTask<T> task, int id) = behaviour.ClientObjectManager.CreateReplyTask<T>();

            message.replyId = id;

            behaviour.Client.Send(message, channelId);

            return task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ServerRpcMessage CreateMessage(NetworkBehaviour behaviour, int hash, NetworkWriter writer, bool requireAuthority)
        {
            RpcMethod rpc = RemoteCallHelper.GetRpc(hash);
            Validate(behaviour, rpc, requireAuthority);

            var message = new ServerRpcMessage
            {
                netId = behaviour.NetId,
                componentIndex = behaviour.ComponentIndex,
                functionHash = hash,
                payload = writer.ToArraySegment()
            };
            return message;
        }

        static void Validate(NetworkBehaviour behaviour, RpcMethod rpc, bool requireAuthority)
        {
            INetworkClient client = behaviour.Client;

            if (client == null || !client.Active)
            {
                throw new InvalidOperationException($"ServerRpc Function {rpc} called on server without an active client.");
            }

            // if authority is required, then client must have authority to send
            if (requireAuthority && !(behaviour.HasAuthority))
            {
                throw new UnauthorizedAccessException($"Trying to send ServerRpc for object without authority. {rpc}");
            }

            if (client.Player == null)
            {
                throw new InvalidOperationException("Send ServerRpc attempted with no client connection.");
            }
        }
    }
}

