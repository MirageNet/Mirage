using System;
using System.Runtime.CompilerServices;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    public static class ClientRpcSender
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientRpcSender));

        public static void Send(NetworkBehaviour behaviour, int hash, NetworkWriter writer, int channelId, bool excludeOwner)
        {
            RpcMessage message = CreateMessage(behaviour, hash, writer);

            // The public facing parameter is excludeOwner in [ClientRpc]
            // so we negate it here to logically align with SendToReady.
            bool includeOwner = !excludeOwner;
            behaviour.Identity.SendToRemoteObservers(message, includeOwner, channelId);
        }

        public static void SendTarget(NetworkBehaviour behaviour, int hash, NetworkWriter writer, int channelId, INetworkPlayer player)
        {
            RpcMessage message = CreateMessage(behaviour, hash, writer);

            // connection parameter is optional. use owner if null
            if (player == null)
            {
                player = behaviour.Owner;
            }

            player.Send(message, channelId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static RpcMessage CreateMessage(NetworkBehaviour behaviour, int hash, NetworkWriter writer)
        {
            RpcMethod rpc = RemoteCallHelper.GetRpc(hash);

            Validate(behaviour, rpc);

            var message = new RpcMessage
            {
                netId = behaviour.NetId,
                componentIndex = behaviour.ComponentIndex,
                functionHash = hash,
                payload = writer.ToArraySegment()
            };
            return message;
        }

        static void Validate(NetworkBehaviour behaviour, RpcMethod rpc)
        {
            INetworkServer server = behaviour.Server;
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
    }
}

