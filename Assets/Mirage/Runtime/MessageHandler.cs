using System;
using System.Collections.Generic;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage
{
    public class MessageHandler : IMessageReceiver
    {
        private static readonly ILogger logger = LogFactory.GetLogger<MessageHandler>();

        private readonly bool _disconnectOnException;
        private readonly bool _rethrowException = false;
        private readonly IObjectLocator _objectLocator;

        internal readonly Dictionary<int, Handler> _messageHandlers = new Dictionary<int, Handler>();

        public MessageHandler(IObjectLocator objectLocator, bool disconnectOnException, bool rethrowException = false)
        {
            _disconnectOnException = disconnectOnException;
            _objectLocator = objectLocator;
            _rethrowException = rethrowException;
        }

        public void RegisterHandler<T>(MessageDelegateWithPlayer<T> handler, bool allowUnauthenticated)
        {
            var msgId = MessagePacker.GetId<T>();

            if (logger.LogEnabled())
            {
                if (_messageHandlers.ContainsKey(msgId))
                    logger.Log($"Replacing handler for id:{msgId} type:{typeof(T)}");
                else
                    logger.Log($"New handler for id:{msgId} type:{typeof(T)}");
            }

            var del = MessageWrapper(handler);
            _messageHandlers[msgId] = new Handler(del, allowUnauthenticated, typeof(T));
        }

        private NetworkMessageDelegate MessageWrapper<T>(MessageDelegateWithPlayer<T> handler)
        {
            void AdapterFunction(INetworkPlayer player, NetworkReader reader)
            {
                T message;
                try
                {
                    message = NetworkDiagnostics.ReadWithDiagnostics<T>(reader);
                }
                catch (Exception e)
                {
                    HandleExceptionInReader(player, e);

                    if (_rethrowException)
                        // note, dont add Exception here, otherwise stack trace will be overwritten
                        throw;
                    else
                        return;
                }

                if (logger.LogEnabled()) logger.Log($"Receiving {typeof(T)} from {player}");

                try
                {
                    handler.Invoke(player, message);
                }
                catch (Exception e)
                {
                    HandleExceptionInMessage(player, e);

                    if (_rethrowException)
                        // note, dont add Exception here, otherwise stack trace will be overwritten
                        throw;
                    else
                        return;
                }
            }
            return AdapterFunction;
        }

        /// <summary>
        /// Unregister a handler for a particular message type.
        /// <para>Note: Messages dont need to be unregister when server or client stops as MessageHandler will be re-created next time server or client starts</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        public void UnregisterHandler<T>()
        {
            var msgType = MessagePacker.GetId<T>();
            _messageHandlers.Remove(msgType);
        }

        /// <summary>
        /// Clear all registered callback handlers.
        /// </summary>
        public void ClearHandlers()
        {
            _messageHandlers.Clear();
        }

        public void HandleMessage(INetworkPlayer player, ArraySegment<byte> packet)
        {
            using (var networkReader = NetworkReaderPool.GetReader(packet, _objectLocator))
            {
                // protect against attackers trying to send invalid data packets
                // exception could be throw if:
                // - invalid headers
                // - invalid message ids
                // - invalid data causing exceptions
                // - negative ReadBytesAndSize prefixes
                // - invalid utf8 strings
                // - etc.
                //
                // if exception is caught, disconnect the attacker to stop any further attacks

                try
                {
                    // we can check if there are enough bytes in reader before calling UnpackId
                    if (!networkReader.CanReadBytes(MessagePacker.ID_BYTE_SIZE))
                    {
                        // cost=50 because NetworkReader throwing means serialization mismatch, hard to recover from, likely need to kick player if it happens often.
                        player.SetError(50, PlayerErrorFlags.DeserializationException);
                        return;
                    }

                    var msgType = MessagePacker.UnpackId(networkReader);
                    InvokeHandler(player, msgType, networkReader);
                }
                catch (Exception e)
                {
                    // TODO can this happen any more? we have try/catch inside MessageWrapper
                    HandleExceptionInMessage(player, e);

                    if (_rethrowException)
                        // note, dont add Exception here, otherwise stack trace will be overwritten
                        throw;
                }
            }
        }

        public void HandleExceptionInReader(INetworkPlayer player, Exception e)
        {
            var disconnectMessage = _disconnectOnException ? $", Closed connection: {player}" : "";
            logger.LogError($"{e.GetType()} in NetworkReader (see stack below){disconnectMessage}\n{e}");

            // cost=50 because NetworkReader throwing means serialization mismatch, hard to recover from, likely need to kick player if it happens often.
            player.SetError(50, PlayerErrorFlags.DeserializationException);

            if (_disconnectOnException)
                player.Disconnect();
        }

        public void HandleExceptionInMessage(INetworkPlayer player, Exception e)
        {
            var disconnectMessage = _disconnectOnException ? $", Closed connection: {player}" : "";
            logger.LogError($"{e.GetType()} in Message handler (see stack below){disconnectMessage}\n{e}");

            // use cost=10 for this because NetworkMessage handler should almost never be throwing
            player.SetError(10, PlayerErrorFlags.RpcException);

            if (_disconnectOnException)
                player.Disconnect();
        }

        internal void InvokeHandler(INetworkPlayer player, int msgType, NetworkReader reader)
        {
            if (_messageHandlers.TryGetValue(msgType, out var handler))
            {
                if (CheckAuthentication(player, msgType, handler))
                    handler.Delegate.Invoke(player, reader);
            }
            else
            {
                int errorCost;
                if (MessagePacker.MessageTypes.TryGetValue(msgType, out var type))
                {
                    errorCost = 20;
                    // this means we received a Message that has a struct, but no handler, It is likely that the developer forgot to register a handler or sent it by mistake
                    // we want this to be warning level
                    if (logger.WarnEnabled()) logger.LogWarning($"Unexpected message {type} received from {player}. Did you register a handler for it?");
                }
                else
                {
                    errorCost = 60;
                    // todo maybe we should handle it differently? we dont want someone spaming ids to find a handler they can do stuff with...
                    if (logger.LogEnabled()) logger.Log($"Unexpected message ID {msgType} received from {player}. May be due to no existing RegisterHandler for this message.");
                }

                // cost=20 because NetworkReader throwing means serialization mismatch, hard to recover from, likely need to kick player if it happens often.
                player.SetError(errorCost, PlayerErrorFlags.RpcSync);
            }
        }

        private bool CheckAuthentication(INetworkPlayer player, int msgType, Handler handler)
        {
            // always allowed
            if (handler.AllowUnauthenticated)
                return true;

            // is authenticated
            if (player.Authentication != null)
                return true;

            // not authenticated
            // log and disconnect

            // player is Unauthenticated so we dont trust them
            // info log only, so attacker can force server to spam logs 
            if (logger.LogEnabled())
            {
                // we know msgType is found (because we have handler), so we dont need if check for tryGet
                MessagePacker.MessageTypes.TryGetValue(msgType, out var type);
                logger.Log($"Unauthenticated Message {type} received from {player}, player is not Authenticated so handler will not be invoked");
            }

            logger.LogError(handler.UnauthenticatedError);
            player.SetErrorAndDisconnect(PlayerErrorFlags.Unauthorized);

            return false;
        }

        /// <summary>
        /// Handles network messages on client and server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reader"></param>
        internal delegate void NetworkMessageDelegate(INetworkPlayer player, NetworkReader reader);

        internal class Handler
        {
            public readonly NetworkMessageDelegate Delegate;
            public readonly bool AllowUnauthenticated;
            public readonly string UnauthenticatedError;

            public Handler(NetworkMessageDelegate @delegate, bool allowUnauthenticated, Type type)
            {
                Delegate = @delegate;
                AllowUnauthenticated = allowUnauthenticated;

                // cache the error message with the type, so we can log it on server with less allocations
                if (!allowUnauthenticated)
                    UnauthenticatedError = $"Unauthenticated Message {type.FullName}, Disconnecting player";
            }
        }
    }
}
