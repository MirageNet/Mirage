using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    public interface IClientRpcSender
    {
        public abstract void Send(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool excludeOwner);
        public abstract void SendTarget(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, INetworkPlayer player);
        public abstract bool ShouldInvokeLocally(NetworkBehaviour behaviour, RpcTarget target, INetworkPlayer player);
    }
    public interface IServerRpcSender
    {
        public abstract void Send(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool requireAuthority);
        public abstract UniTask<T> SendWithReturn<T>(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool requireAuthority);
        public abstract bool ShouldInvokeLocally(NetworkBehaviour behaviour, bool requireAuthority);
    }

    public class ClientRpcSender : IClientRpcSender
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientRpcSender));

        public void Send(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool excludeOwner)
        {
            var identity = behaviour.Identity;
            var index = behaviour.Identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, index);

            var message = CreateMessage(identity, index, writer);

            // The public facing parameter is excludeOwner in [ClientRpc]
            // so we negate it here to logically align with SendToReady.
            var includeOwner = !excludeOwner;
            identity.SendToRemoteObservers(message, includeOwner, channelId);
        }

        public void SendTarget(ICanHaveRpc behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, INetworkPlayer player)
        {
            var identity = behaviour.Identity;
            var index = behaviour.Identity.RemoteCallCollection.GetIndexOffset(behaviour) + relativeIndex;
            Validate(behaviour, index);

            var message = CreateMessage(identity, index, writer);

            // player parameter is optional. use owner if null
            if (player == null)
            {
                player = identity.Owner;
            }

            // if still null throw to give useful error
            if (player == null)
            {
                throw new InvalidOperationException("Player target was null for Rpc");
            }

            player.Send(message, channelId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RpcMessage CreateMessage(INetworkIdentity identity, int index, NetworkWriter writer)
        {
            var message = new RpcMessage
            {
                NetId = identity.NetId,
                FunctionIndex = index,
                Payload = writer.ToArraySegment()
            };
            return message;
        }

        private static void Validate(ICanHaveRpc behaviour, int index)
        {
            var identity = behaviour.Identity;

            var server = identity.Server;
            if (server == null || !server.Active)
            {
                var rpc = identity.RemoteCallCollection.GetRelative(behaviour, index);
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
        public bool ShouldInvokeLocally(NetworkBehaviour behaviour, RpcTarget target, INetworkPlayer player)
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

