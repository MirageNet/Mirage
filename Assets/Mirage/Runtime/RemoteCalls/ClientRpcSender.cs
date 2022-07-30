using System;
using System.Runtime.CompilerServices;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    public static class ClientRpcSender
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientRpcSender));

        public static void Send(NetworkBehaviour behaviour, int index, NetworkWriter writer, int channelId, bool excludeOwner)
        {
            var message = CreateMessage(behaviour, index, writer);

            // The public facing parameter is excludeOwner in [ClientRpc]
            // so we negate it here to logically align with SendToReady.
            var includeOwner = !excludeOwner;
            behaviour.Identity.SendToRemoteObservers(message, includeOwner, channelId);
        }

        public static void SendTarget(NetworkBehaviour behaviour, int index, NetworkWriter writer, int channelId, INetworkPlayer player)
        {
            var message = CreateMessage(behaviour, index, writer);

            // player parameter is optional. use owner if null
            if (player == null)
            {
                player = behaviour.Owner;
            }

            // if still null throw to give useful error
            if (player == null)
            {
                throw new InvalidOperationException("Player target was null for Rpc");
            }

            player.Send(message, channelId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RpcMessage CreateMessage(NetworkBehaviour behaviour, int index, NetworkWriter writer)
        {
            var rpc = behaviour.remoteCallCollection.Get(index);

            Validate(behaviour, rpc);

            var message = new RpcMessage
            {
                netId = behaviour.NetId,
                componentIndex = behaviour.ComponentIndex,
                functionIndex = index,
                payload = writer.ToArraySegment()
            };
            return message;
        }

        private static void Validate(NetworkBehaviour behaviour, RemoteCall rpc)
        {
            var server = behaviour.Server;
            if (server == null || !server.Active)
            {
                throw new InvalidOperationException($"RPC Function {rpc} called when server is not active.");
            }

            // This cannot use Server.active, as that is not specific to this object.
            if (!behaviour.IsServer)
            {
                if (logger.WarnEnabled()) logger.LogWarning($"ClientRpc {rpc} called on un-spawned object: {behaviour.name}");
                return;
            }
        }

        /// <summary>
        /// Checks if host player can see the object
        /// <para>Weaver uses this to check if RPC should be invoked locally</para>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsLocalPlayerObserver(NetworkBehaviour behaviour)
        {
            if (behaviour.Server != null)
            {
                var local = behaviour.Server.LocalPlayer;
                return behaviour.Identity.observers.Contains(local);
            }

            // todo should ClientRpc be called in client only mode
            return true;
        }

        /// <summary>
        /// Checks if host player is the target player
        /// <para>Weaver uses this to check if RPC should be invoked locally</para>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsLocalPlayerTarget(NetworkBehaviour behaviour, INetworkPlayer target)
        {
            if (behaviour.Server != null)
            {
                var local = behaviour.Server.LocalPlayer;
                return local == target;
            }

            // todo should ClientRpc be called in client only mode
            return true;
        }
    }
}

