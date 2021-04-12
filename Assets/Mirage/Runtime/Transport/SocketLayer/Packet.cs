using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Received packet
    /// <para>contains raw data and helper methods to help process that data</para>
    /// </summary>
    internal struct Packet
    {
        const int MinPacketSize = 1;
        const int MinCommandSize = 2;
        const int MinNotifySize = 1 + 2 + 2 + 4;
        /// <summary>
        /// Min size of message given to Mirage
        /// </summary>
        const int MinUnreliableSize = 3;

        public ByteBuffer buffer;
        public int length;

        public Packet(ByteBuffer data, int length)
        {
            buffer = data ?? throw new ArgumentNullException(nameof(data));
            this.length = length;
        }

        public bool IsValidSize()
        {
            if (length < MinPacketSize)
                return false;

            switch (type)
            {
                case PacketType.Command:
                    return length >= MinCommandSize;

                case PacketType.Unreliable:
                    return length >= MinUnreliableSize;

                case PacketType.Notify:
                    return length >= MinNotifySize;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }

        public PacketType type => (PacketType)buffer.array[0];
        public Commands command => (Commands)buffer.array[1];

        public ArraySegment<byte> ToSegment()
        {
            // ingore packet type
            return new ArraySegment<byte>(buffer.array, 1, length);
        }
    }
}
