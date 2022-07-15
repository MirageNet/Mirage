using System;
using System.Collections.Generic;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage
{
    public class MessageHandler : IMessageReceiver
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(MessageHandler));

        private readonly bool _disconnectOnException;
        private readonly IObjectLocator _objectLocator;

        /// <summary>
        /// Handles network messages on client and server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reader"></param>
        internal delegate void NetworkMessageDelegate(INetworkPlayer player, NetworkReader reader);

        internal readonly Dictionary<int, NetworkMessageDelegate> _messageHandlers = new Dictionary<int, NetworkMessageDelegate>();

        public MessageHandler(IObjectLocator objectLocator, bool disconnectOnException)
        {
            _disconnectOnException = disconnectOnException;
            _objectLocator = objectLocator;
        }

        private static NetworkMessageDelegate MessageWrapper<T>(MessageDelegateWithPlayer<T> handler)
        {
            void AdapterFunction(INetworkPlayer player, NetworkReader reader)
            {
                var message = NetworkDiagnostics.ReadWithDiagnostics<T>(reader);

                handler.Invoke(player, message);
            }
            return AdapterFunction;
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        public void RegisterHandler<T>(MessageDelegateWithPlayer<T> handler)
        {
            var msgType = MessagePacker.GetId<T>();
            if (logger.filterLogType == LogType.Log && _messageHandlers.ContainsKey(msgType))
            {
                logger.Log($"RegisterHandler replacing {msgType}");
            }
            _messageHandlers[msgType] = MessageWrapper(handler);
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        public void RegisterHandler<T>(MessageDelegate<T> handler)
        {
            RegisterHandler<T>((_, value) => handler.Invoke(value));
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


        internal void InvokeHandler(INetworkPlayer player, int msgType, NetworkReader reader)
        {
            if (_messageHandlers.TryGetValue(msgType, out var msgDelegate))
            {
                msgDelegate.Invoke(player, reader);
            }
            else
            {
                if (MessagePacker.MessageTypes.TryGetValue(msgType, out var type))
                {
                    // this means we received a Message that has a struct, but no handler, It is likely that the developer forgot to register a handler or sent it by mistake
                    // we want this to be warning level
                    if (logger.WarnEnabled()) logger.LogWarning($"Unexpected message {type} received from {player}. Did you register a handler for it?");
                }
                else
                {
                    // todo maybe we should handle it differently? we dont want someone spaming ids to find a handler they can do stuff with...
                    if (logger.LogEnabled()) logger.Log($"Unexpected message ID {msgType} received from {player}. May be due to no existing RegisterHandler for this message.");
                }
            }
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
                    var msgType = MessagePacker.UnpackId(networkReader);
                    InvokeHandler(player, msgType, networkReader);
                }
                catch (Exception e)
                {
                    var disconnectMessage = _disconnectOnException ? $", Closed connection: {player}" : "";
                    logger.LogError($"{e.GetType()} in Message handler (see stack below){disconnectMessage}\n{e}");
                    if (_disconnectOnException)
                    {
                        player.Disconnect();
                    }
                }
            }
        }
    }
}
