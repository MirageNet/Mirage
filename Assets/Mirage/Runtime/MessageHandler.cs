using System;
using System.Collections.Generic;
using System.IO;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage
{
    public class MessageHandler : IMessageReceiver
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(MessageHandler));

        readonly bool KickPlayerOnException;

        // message handlers for this connection
        internal readonly Dictionary<int, NetworkMessageDelegate> messageHandlers = new Dictionary<int, NetworkMessageDelegate>();

        public MessageHandler(bool kickPlayerOnException)
        {
            KickPlayerOnException = kickPlayerOnException;
        }

        // Handles network messages on client and server
        internal delegate void NetworkMessageDelegate(INetworkPlayer player, NetworkReader reader);
        
        private static NetworkMessageDelegate MessageWrapper<T>(Action<INetworkPlayer, T> handler)
        {
            void AdapterFunction(INetworkPlayer player, NetworkReader reader)
            {
                T message = NetworkDiagnostics.ReadWithDiagnostics<T>(reader);

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
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<INetworkPlayer, T> handler)
        {
            int msgType = MessagePacker.GetId<T>();
            if (logger.filterLogType == LogType.Log && messageHandlers.ContainsKey(msgType))
            {
                logger.Log("RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = MessageWrapper(handler);
        }

        /// <summary>
        /// Register a handler for a particular message type.
        /// <para>There are several system message types which you can add handlers for. You can also add your own message types.</para>
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="handler">Function handler which will be invoked for when this message type is received.</param>
        /// <param name="requireAuthentication">True if the message requires an authenticated connection</param>
        public void RegisterHandler<T>(Action<T> handler)
        {
            RegisterHandler<T>((_, value) => { handler(value); });
        }

        /// <summary>
        /// Unregister a handler for a particular message type.
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        public void UnregisterHandler<T>()
        {
            int msgType = MessagePacker.GetId<T>();
            messageHandlers.Remove(msgType);
        }

        /// <summary>
        /// Clear all registered callback handlers.
        /// </summary>
        public void ClearHandlers()
        {
            messageHandlers.Clear();
        }


        internal void InvokeHandler(INetworkPlayer player, int msgType, NetworkReader reader)
        {
            if (messageHandlers.TryGetValue(msgType, out NetworkMessageDelegate msgDelegate))
            {
                msgDelegate.Invoke(player, reader);
            }
            else
            {
                try
                {
                    Type type = MessagePacker.GetMessageType(msgType);
                    throw new InvalidDataException($"Unexpected message {type} received in {this}. Did you register a handler for it?");
                }
                catch (KeyNotFoundException)
                {
                    throw new InvalidDataException($"Unexpected message ID {msgType} received in {this}. May be due to no existing RegisterHandler for this message.");
                }
            }
        }

        public void HandleMessage(INetworkPlayer player, ArraySegment<byte> packet)
        {
            using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(packet))
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
                    int msgType = MessagePacker.UnpackId(networkReader);
                    InvokeHandler(msgType, networkReader);
                }
                catch (InvalidDataException ex)
                {
                    logger.Log(ex.ToString());
                }
                catch (Exception e)
                {
                    logger.LogError($"{e.GetType()} in Message handler (see stack below), Closed connection: {this}\n{e}");
                    Connection?.Disconnect();
                }
            }
        }
    }
}
