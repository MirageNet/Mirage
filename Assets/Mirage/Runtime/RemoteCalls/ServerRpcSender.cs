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
        public static void Send(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool requireAuthority)
        {
            var absoluteIndex = behaviour.Identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, absoluteIndex, requireAuthority);

            var message = new RpcMessage
            {
                NetId = behaviour.NetId,
                FunctionIndex = absoluteIndex,
                Payload = writer.ToArraySegment()
            };

            behaviour.Client.Send(message, channelId);
        }

        public static UniTask<T> SendWithReturn<T>(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, bool requireAuthority)
        {
            var collection = behaviour.Identity.RemoteCallCollection;
            var absoluteIndex = collection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, absoluteIndex, requireAuthority);

            var message = new RpcWithReplyMessage
            {
                NetId = behaviour.NetId,
                FunctionIndex = absoluteIndex,
                Payload = writer.ToArraySegment()
            };

            var callInfo = collection.GetAbsolute(absoluteIndex);
            (var task, var id) = behaviour.ClientObjectManager._rpcHandler.CreateReplyTask<T>(callInfo, behaviour.Client.Player);

            message.ReplyId = id;

            // reply rpcs are always reliable
            behaviour.Client.Send(message, Channel.Reliable);

            return task;
        }

        private static void Validate(NetworkBehaviour behaviour, int absoluteIndex, bool requireAuthority)
        {
            var client = behaviour.Client;
            var collection = behaviour.Identity.RemoteCallCollection;
            var callInfo = collection.GetAbsolute(absoluteIndex);

            if (client == null || !client.Active)
            {
                throw new InvalidOperationException($"ServerRpc Function {callInfo} called on server without an active client.");
            }

            // if authority is required, then client must have authority to send
            if (requireAuthority && !behaviour.HasAuthority)
            {
                throw new InvalidOperationException($"Trying to send ServerRpc for object without authority. {callInfo}");
            }

            if (client.Player == null)
            {
                throw new InvalidOperationException("Send ServerRpc attempted with no client connection.");
            }

            if (callInfo.RateLimit.IsEnabled)
            {
                var allowed = client.Player.CheckRateLimit(callInfo);
                if (!allowed)
                {
                    var errorMsg = $"ServerRpc '{callInfo.Name}' was rate limited on the client and not sent.";
                    throw new ReturnRpcException(errorMsg);
                }
            }
        }

        /// <summary>
        /// Used by weaver to check if ClientRPC should be invoked locally in host mode
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="target"></param>
        /// <param name="player">player used for RpcTarget.Player</param>
        /// <returns></returns>
        public static bool ShouldInvokeLocally(NetworkBehaviour behaviour, bool requireAuthority, bool allowServerToCall)
        {
            // if allowServerToCall, then just check server because we ignore all other checks
            if (behaviour.IsServer && allowServerToCall)
                return true;

            // not client? error
            if (!behaviour.IsClient)
                throw new InvalidOperationException("Server RPC can only be called when client is active");

            // not host? never invoke locally
            if (!behaviour.IsServer)
                return false;

            // check if auth is required and that host has auth over the object
            if (requireAuthority && !behaviour.HasAuthority)
                throw new InvalidOperationException($"Trying to send ServerRpc for object without authority.");

            return true;
        }
    }
}

