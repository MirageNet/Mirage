using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    public static class ClientRpcSender
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientRpcSender));

        public static void Send(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool excludeOwner)
        {
            var index = behaviour.Identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, index);

            var message = CreateMessage(behaviour, index, writer);

            // The public facing parameter is excludeOwner in [ClientRpc]
            // so we negate it here to logically align with SendToReady.
            var includeOwner = !excludeOwner;
            behaviour.Identity.SendToRemoteObservers(message, includeOwner, channelId);
        }

        public static void SendTarget(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, INetworkPlayer player)
        {
            var index = behaviour.Identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, index);

            var message = CreateMessage(behaviour, index, writer);

            player = GetTarget(behaviour, player);

            player.Send(message, channelId);
        }

        public static UniTask<T> SendTargetWithReturn<T>(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, INetworkPlayer player)
        {
            var index = behaviour.Identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, index);

            (var task, var id) = behaviour.ServerObjectManager._rpcHandler.CreateReplyTask<T>();
            var message = new RpcWithReplyMessage
            {
                NetId = behaviour.NetId,
                FunctionIndex = index,
                ReplyId = id,
                Payload = writer.ToArraySegment()
            };

            player = GetTarget(behaviour, player);

            // reply rpcs are always reliable
            player.Send(message, Channel.Reliable);

            return task;
        }

        private static INetworkPlayer GetTarget(NetworkBehaviour behaviour, INetworkPlayer player)
        {
            // player parameter is optional. use owner if null
            if (player == null)
                player = behaviour.Owner;

            // if still null throw to give useful error
            if (player == null)
                throw new InvalidOperationException("Player target was null for Rpc");

            return player;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RpcMessage CreateMessage(NetworkBehaviour behaviour, int index, NetworkWriter writer)
        {
            var message = new RpcMessage
            {
                NetId = behaviour.NetId,
                FunctionIndex = index,
                Payload = writer.ToArraySegment()
            };
            return message;
        }

        private static void Validate(NetworkBehaviour behaviour, int index)
        {
            var server = behaviour.Server;
            if (server == null || !server.Active)
            {
                var rpc = behaviour.Identity.RemoteCallCollection.GetRelative(behaviour, index);
                throw new InvalidOperationException($"RPC Function {rpc} called when server is not active.");
            }
        }

        /// <summary>
        /// Used by weaver to check if ClientRPC should be invoked locally in host mode
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="target"></param>
        /// <param name="player">player used for RpcTarget.Player</param>
        /// <returns></returns>
        public static bool ShouldInvokeLocally(NetworkBehaviour behaviour, RpcTarget target, INetworkPlayer player, bool excludeOwner)
        {
            // not server? error
            if (!behaviour.IsServer)
            {
                throw new InvalidOperationException("Client RPC can only be called when server is active");
            }

            // not host? never invoke locally
            if (!behaviour.IsClient)
                return false;

            // check if host player should receive
            switch (target)
            {
                case RpcTarget.Observers:
                    return IsLocalPlayerObserver(behaviour, excludeOwner);
                case RpcTarget.Owner:
                    return IsLocalPlayerTarget(behaviour, behaviour.Owner);
                case RpcTarget.Player:
                    return IsLocalPlayerTarget(behaviour, player);
            }

            // should never get here
            throw new InvalidEnumArgumentException();
        }

        /// <summary>
        /// Checks if host player can see the object
        /// <para>Weaver uses this to check if RPC should be invoked locally</para>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsLocalPlayerObserver(NetworkBehaviour behaviour, bool excludeOwner)
        {
            var local = behaviour.Server.LocalPlayer;

            // if local player is the owner, skip
            if (excludeOwner && behaviour.Owner == local)
                return false;

            return behaviour.Identity.observers.Contains(local);
        }

        /// <summary>
        /// Checks if host player is the target player
        /// <para>Weaver uses this to check if RPC should be invoked locally</para>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsLocalPlayerTarget(NetworkBehaviour behaviour, INetworkPlayer target)
        {
            var local = behaviour.Server.LocalPlayer;
            return local == target;
        }
    }
}

