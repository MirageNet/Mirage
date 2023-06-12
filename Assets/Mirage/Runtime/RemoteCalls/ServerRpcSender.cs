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
        public static void Send(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool requireAuthority)
        {
            var identity = behaviour.Identity;
            var index = identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, index, requireAuthority);

            var message = new ServerRpcMessage
            {
                NetId = identity.NetId,
                FunctionIndex = index,
                Payload = writer.ToArraySegment()
            };

            identity.Client.Send(message, channelId);
        }

        public static UniTask<T> SendWithReturn<T>(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool requireAuthority)
        {
            var identity = behaviour.Identity;
            var index = identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, index, requireAuthority);

            var message = new ServerRpcWithReplyMessage
            {
                NetId = identity.NetId,
                FunctionIndex = index,
                Payload = writer.ToArraySegment()
            };

            (var task, var id) = identity.ClientObjectManager.CreateReplyTask<T>();

            message.ReplyId = id;

            identity.Client.Send(message, channelId);

            return task;
        }

        private static void Validate(ICanHaveRpc behaviour, int index, bool requireAuthority)
        {
            var identity = behaviour.Identity;
            var client = identity.Client;

            if (client == null || !client.Active)
            {
                var rpc = identity.RemoteCallCollection.GetRelative(behaviour, index);
                throw new InvalidOperationException($"ServerRpc Function {rpc} called on server without an active client.");
            }

            // if authority is required, then client must have authority to send
            if (requireAuthority && !identity.HasAuthority)
            {
                var rpc = identity.RemoteCallCollection.GetRelative(behaviour, index);
                throw new InvalidOperationException($"Trying to send ServerRpc for object without authority. {rpc}");
            }

            if (client.Player == null)
            {
                throw new InvalidOperationException("Send ServerRpc attempted with no client connection.");
            }
        }

        /// <summary>
        /// Used by weaver to check if ClientRPC should be invoked locally in host mode
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="target"></param>
        /// <param name="player">player used for RpcTarget.Player</param>
        /// <returns></returns>
        public static bool ShouldInvokeLocally(NetworkBehaviour behaviour, bool requireAuthority)
        {
            // not client? error
            if (!behaviour.IsClient)
            {
                throw new InvalidOperationException("Server RPC can only be called when client is active");
            }

            // not host? never invoke locally
            if (!behaviour.IsServer)
                return false;

            // check if auth is required and that host has auth over the object
            if (requireAuthority && !behaviour.HasAuthority)
            {
                throw new InvalidOperationException($"Trying to send ServerRpc for object without authority.");
            }

            return true;
        }
    }
}

