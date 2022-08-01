using System;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;

namespace Mirage.RemoteCalls
{
    /// <summary>
    /// Methods used by weaver to send RPCs
    /// </summary>
    public static class ServerRpcSender
    {
        public static void Send(NetworkBehaviour behaviour, int index, NetworkWriter writer, int channelId, bool requireAuthority)
        {
            Validate(behaviour, index, requireAuthority);

            var message = new ServerRpcMessage
            {
                netId = behaviour.NetId,
                componentIndex = behaviour.ComponentIndex,
                functionIndex = index,
                payload = writer.ToArraySegment()
            };

            behaviour.Client.Send(message, channelId);
        }

        public static UniTask<T> SendWithReturn<T>(NetworkBehaviour behaviour, int index, NetworkWriter writer, int channelId, bool requireAuthority)
        {
            Validate(behaviour, index, requireAuthority);
            var message = new ServerRpcWithReplyMessage
            {
                netId = behaviour.NetId,
                componentIndex = behaviour.ComponentIndex,
                functionIndex = index,
                payload = writer.ToArraySegment()
            };

            (var task, var id) = behaviour.ClientObjectManager.CreateReplyTask<T>();

            message.replyId = id;

            behaviour.Client.Send(message, channelId);

            return task;
        }

        private static void Validate(NetworkBehaviour behaviour, int index, bool requireAuthority)
        {
            var rpc = behaviour.remoteCallCollection.Get(index);
            var client = behaviour.Client;

            if (client == null || !client.Active)
            {
                throw new InvalidOperationException($"ServerRpc Function {rpc} called on server without an active client.");
            }

            // if authority is required, then client must have authority to send
            if (requireAuthority && !behaviour.HasAuthority)
            {
                throw new InvalidOperationException($"Trying to send ServerRpc for object without authority. {rpc}");
            }

            if (client.Player == null)
            {
                throw new InvalidOperationException("Send ServerRpc attempted with no client connection.");
            }
        }
    }
}

