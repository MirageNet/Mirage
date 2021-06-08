using System;
using Mirage.Serialization;

namespace Mirage
{
    public static class LargeMessageSender
    {
        /// <summary>
        /// Sends message that is over the MTU
        /// <para>Sends message using reliable ordered channel so that they will all be received in order.
        /// This means that we dont need to send message Id or worry about </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="player">target player</param>
        /// <param name="msg">large message</param>
        /// <param name="MTU">Should be max part size, take into account header size for message</param>
        /// <param name="messageSize">Expected message size (can be over estimate)</param>
        public static void Send<T>(INetworkPlayer player, T msg, int MTU = 1200, int messageSize = default)
        {
            // todo use maxSize for bitwriter
            var writer = new NetworkWriter();
            MessagePacker.Pack(msg, writer);

            var segment = writer.ToArraySegment();
            int size = segment.Count;
            player.Send(new LargeMessageStart { totalSize = size });

            int fullParts = size / MTU;
            for (int i = 0; i < fullParts; i++)
            {
                int offset = segment.Offset + i * MTU;
                int count = MTU;
                player.Send(new LargeMessagePart
                {
                    data = new ArraySegment<byte>(segment.Array, offset, count)
                });
            }
            // round
            int leftOver = size - (fullParts * MTU);
            player.Send(new LargeMessageEnd
            {
                lastPart = new ArraySegment<byte>(segment.Array, segment.Offset + fullParts * MTU, leftOver)
            });
        }


        static NetworkWriter writer;
        static void ReceiveStart(INetworkPlayer player, LargeMessageStart start)
        {
            if (writer != null) throw new InvalidOperationException("Large Message already started");

            // todo use maxSize for bitwriter
            writer = new NetworkWriter();
        }
        static void ReceivePart(INetworkPlayer player, LargeMessagePart part)
        {
            if (writer == null) throw new InvalidOperationException("Large Message not started");

            writer.WriteArraySegment(part.data);
        }
        static void ReceiveEnd(INetworkPlayer player, LargeMessageEnd end)
        {
            if (writer == null) throw new InvalidOperationException("Large Message not started");

            writer.WriteArraySegment(end.lastPart);

            var fullMessage = writer.ToArraySegment();
            if (player is IMessageHandler handler)
            {
                handler.HandleMessage(fullMessage);
            }
            else
            {
                throw new NotSupportedException("Player is not a message receiver");
            }

            // clear
            writer = null;
        }

        public static void RegisterMessageHandlers(INetworkPlayer player)
        {
            player.RegisterHandler<LargeMessageStart>(ReceiveStart);
            player.RegisterHandler<LargeMessagePart>(ReceivePart);
            player.RegisterHandler<LargeMessageEnd>(ReceiveEnd);
        }
    }
}
namespace Mirage.Serialization
{
    [NetworkMessage]
    public struct LargeMessageStart
    {
        public int totalSize;
    }
    [NetworkMessage]
    public struct LargeMessageEnd
    {
        public ArraySegment<byte> lastPart;
    }
    [NetworkMessage]
    public struct LargeMessagePart
    {
        public ArraySegment<byte> data;
    }
}
