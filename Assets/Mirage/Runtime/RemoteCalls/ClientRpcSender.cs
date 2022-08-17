using System;
using System.ComponentModel;
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
            var rpc = behaviour.RemoteCallCollection.Get(index);

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
        }

        /// <summary>
        /// Used by weaver to check if ClientRPC should be invoked locally in host mode
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="target"></param>
        /// <param name="player">player used for RpcTarget.Player</param>
        /// <returns></returns>
        public static bool ShouldInvokeLocally(NetworkBehaviour behaviour, RpcTarget target, INetworkPlayer player)
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
                    return IsLocalPlayerObserver(behaviour);
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
        public static bool IsLocalPlayerObserver(NetworkBehaviour behaviour)
        {
            var local = behaviour.Server.LocalPlayer;
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

