using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    internal class RpcHandler
    {
        private static readonly ILogger logger = LogFactory.GetLogger<RpcHandler>();

        private readonly Dictionary<int, Action<NetworkReader>> _callbacks = new Dictionary<int, Action<NetworkReader>>();
        private int _nextReplyId;
        /// <summary>
        /// Object locator required for deserializing the reply
        /// </summary>
        private readonly IObjectLocator _objectLocator;
        /// <summary>
        /// Invoke type for validation
        /// </summary>
        private readonly RpcInvokeType _invokeType;

        public RpcHandler(MessageHandler messageHandler, IObjectLocator objectLocator, RpcInvokeType invokeType)
        {
            messageHandler.RegisterHandler<RpcReply>(OnReply);
            messageHandler.RegisterHandler<RpcMessage>(OnRpcMessage);
            messageHandler.RegisterHandler<RpcWithReplyMessage>(OnRpcWithReplyMessage);

            _objectLocator = objectLocator;
            _invokeType = invokeType;
        }

        /// <summary>
        /// Handle ServerRpc from specific player, this could be one of multiple players on a single client
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        private void OnRpcWithReplyMessage(INetworkPlayer player, RpcWithReplyMessage msg)
        {
            HandleRpc(player, msg.NetId, msg.FunctionIndex, msg.Payload, msg.ReplyId);
        }

        internal void OnRpcMessage(INetworkPlayer player, RpcMessage msg)
        {
            HandleRpc(player, msg.NetId, msg.FunctionIndex, msg.Payload, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleRpc(INetworkPlayer player, uint netId, int functionIndex, ArraySegment<byte> payload, int replyId)
        {
            if (!_objectLocator.TryGetIdentity(netId, out var identity))
            {
                if (logger.WarnEnabled()) logger.LogWarning($"Spawned object not found when handling ServerRpc message [netId={netId}]");
                return;
            }

            var remoteCall = identity.RemoteCallCollection.GetAbsolute(functionIndex);

            if (remoteCall.InvokeType != _invokeType)
                ThrowInvalidRpc(remoteCall);

            // for ServerRpc we need to check if the player has authority
            if (_invokeType == RpcInvokeType.ServerRpc)
            {
                var ok = CheckAuthority(remoteCall, identity, player);
                if (!ok)
                    return;
            }

            if (logger.LogEnabled()) logger.Log($"Rpc for {identity} from {player}");

            using (var reader = NetworkReaderPool.GetReader(payload, _objectLocator))
            {
                remoteCall.Invoke(reader, player, replyId);
            }
        }

        private bool CheckAuthority(RemoteCall remoteCall, NetworkIdentity identity, INetworkPlayer player)
        {
            // not required, return ok
            if (!remoteCall.RequireAuthority)
                return true;

            // is owner, return ok
            if (identity.Owner == player)
                return true;

            // not ok
            if (logger.WarnEnabled()) logger.LogWarning($"ServerRpc for object without authority {identity}");
            return false;
        }

        private void ThrowInvalidRpc(RemoteCall remoteCall)
        {
            throw new MethodInvocationException($"Invalid Rpc for index {remoteCall.Name}. Expected {_invokeType} but was {remoteCall.InvokeType}");
        }

        /// <summary>
        /// Creates a task that waits for a reply from the server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>the task that will be completed when the result is in, and the id to use in the request</returns>
        public (UniTask<T> task, int replyId) CreateReplyTask<T>()
        {
            var newReplyId = _nextReplyId++;
            var completionSource = AutoResetUniTaskCompletionSource<T>.Create();
            void Callback(NetworkReader reader)
            {
                var result = reader.Read<T>();
                completionSource.TrySetResult(result);
            }

            _callbacks.Add(newReplyId, Callback);
            return (completionSource.Task, newReplyId);
        }

        private void OnReply(INetworkPlayer player, RpcReply reply)
        {
            // find the callback that was waiting for this and invoke it.
            if (_callbacks.TryGetValue(reply.ReplyId, out var action))
            {
                _callbacks.Remove(_nextReplyId);
                using (var reader = NetworkReaderPool.GetReader(reply.Payload, _objectLocator))
                {
                    action.Invoke(reader);
                }
            }
            else
            {
                throw new MethodAccessException("Received reply but no handler was registered");
            }
        }
    }
}
