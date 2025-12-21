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
            if (payload.Array == null)
            {
                player.SetError(50, PlayerErrorFlags.DeserializationException);
                if (logger.WarnEnabled()) logger.LogWarning($"HandleRpc was given null array");
                return;
            }

            if (!_objectLocator.TryGetIdentity(netId, out var identity))
            {
                // cost=1 we dont want users spamming this, but also likely to happen if server has just destroyed the object in recent frames
                player.SetError(1, PlayerErrorFlags.None);

                if (logger.WarnEnabled()) logger.LogWarning($"Spawned object not found when handling ServerRpc message [netId={netId}]");
                return;
            }

            var remoteCall = identity.RemoteCallCollection.GetAbsolute(functionIndex);
            if (remoteCall == null)
            {
                player.SetError(50, PlayerErrorFlags.RpcSync);
                if (logger.WarnEnabled()) logger.LogWarning($"Invalid Rpc for index. Out of bounds");
                return;
            }

            if (remoteCall.InvokeType != _invokeType)
            {
                player.SetError(50, PlayerErrorFlags.RpcSync);
                if (logger.WarnEnabled()) logger.LogWarning($"Invalid Rpc for index {remoteCall.Name}. Expected {_invokeType} but was {remoteCall.InvokeType}");
                return;
            }

            // for ServerRpc we need to check if the player has authority
            if (_invokeType == RpcInvokeType.ServerRpc)
            {
                var ok = CheckAuthority(remoteCall, identity, player);
                if (!ok)
                {
                    if (logger.WarnEnabled()) logger.LogWarning($"ServerRpc for object without authority {identity}");
                    player.SetError(10, PlayerErrorFlags.NoAuthority);
                    return;
                }
            }

            if (logger.LogEnabled()) logger.Log($"Rpc for {identity} from {player}");

            using (var reader = NetworkReaderPool.GetReader(payload, _objectLocator))
            {
                try
                {
                    remoteCall.Invoke(reader, player, replyId);
                }
                catch (System.IO.EndOfStreamException e)
                {
                    logger.LogError($"RPC threw EndOfStreamException: {e}");

                    // cost=50 because NetworkReader throwing means serialization mismatch, hard to recover from, likely need to kick player if it happens often.
                    player.SetError(50, PlayerErrorFlags.DeserializationException);
                }
                catch (Exception e)
                {
                    logger.LogError($"RPC threw an Exception: {e}");

                    // Common errors caused by developer mistake
                    if (e is NullReferenceException || e is UnityEngine.MissingReferenceException || e is UnityEngine.UnassignedReferenceException)
                        player.SetError(1, PlayerErrorFlags.RpcNullException);
                    else
                        player.SetError(2, PlayerErrorFlags.RpcException);
                }
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
            return false;
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
            if (_callbacks.Remove(reply.ReplyId, out var callbacks))
            {
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
                logger.LogError($"Received RpcReply from {player} but no pending callbacks for id={reply.ReplyId}");
                // TODO do we need a flag for errors like this?
                //      for actions that are not allowed due to invalid state
                player.SetError(10, PlayerErrorFlags.None);
            }
        }
    }
}
