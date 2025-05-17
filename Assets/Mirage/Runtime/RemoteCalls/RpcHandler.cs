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

        private delegate void ReplyCallbackSuccess(NetworkReader reader);
        private delegate void ReplyCallbackFail();

        private readonly Dictionary<int, (ReplyCallbackSuccess success, ReplyCallbackFail fail)> _callbacks = new Dictionary<int, (ReplyCallbackSuccess success, ReplyCallbackFail fail)>();
        private int _nextReplyId;
        /// <summary>
        /// Object locator required for deserializing the reply
        /// </summary>
        private readonly IObjectLocator _objectLocator;
        /// <summary>
        /// Invoke type for validation
        /// </summary>
        private readonly RpcInvokeType _invokeType;

        public RpcHandler(IObjectLocator objectLocator, RpcInvokeType invokeType)
        {
            _objectLocator = objectLocator;
            _invokeType = invokeType;
        }

        // note: client handles message in ClientObjectManager
        public void ServerRegisterHandler(MessageHandler messageHandler)
        {
            messageHandler.RegisterHandler<RpcReply>(OnReply);
            messageHandler.RegisterHandler<RpcMessage>(OnRpcMessage);
            messageHandler.RegisterHandler<RpcWithReplyMessage>(OnRpcWithReplyMessage);
        }

        /// <summary>
        /// Handle ServerRpc from specific player, this could be one of multiple players on a single client
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        internal void OnRpcWithReplyMessage(INetworkPlayer player, RpcWithReplyMessage msg)
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
        public (UniTask<T> task, int replyId) CreateReplyTask<T>(RemoteCall info)
        {
            var newReplyId = _nextReplyId++;
            var completionSource = AutoResetUniTaskCompletionSource<T>.Create();
            void CallbackSuccess(NetworkReader reader)
            {
                var result = reader.Read<T>();
                completionSource.TrySetResult(result);
            }

            void CallbackFail()
            {
                var netId = 0u;
                var name = "";
                if (info.Behaviour != null)
                {
                    netId = info.Behaviour.NetId;
                    name = info.Behaviour.name;
                }
                var message = $"Exception thrown from return RPC. {info.Name} on netId={netId} {name}";
                completionSource.TrySetException(new ReturnRpcException(message));
            }

            _callbacks.Add(newReplyId, (CallbackSuccess, CallbackFail));
            return (completionSource.Task, newReplyId);
        }

        internal void OnReply(INetworkPlayer player, RpcReply reply)
        {
            // find the callback that was waiting for this and invoke it.
            if (_callbacks.TryGetValue(reply.ReplyId, out var callbacks))
            {
                _callbacks.Remove(_nextReplyId);

                if (reply.Success)
                {
                    using (var reader = NetworkReaderPool.GetReader(reply.Payload, _objectLocator))
                    {
                        callbacks.success.Invoke(reader);
                    }
                }
                else
                {
                    callbacks.fail.Invoke();
                }
            }
            else
            {
                throw new MethodAccessException("Received reply but no handler was registered");
            }
        }
    }
}
