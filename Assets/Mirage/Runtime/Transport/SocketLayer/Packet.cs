using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Received packet
    /// <para>contains raw data and helper methods to help process that data</para>
    /// </summary>
    internal struct Packet
    {
        public readonly ByteBuffer buffer;
        public readonly int length;

        public Packet(ByteBuffer data, int length)
        {
            buffer = data ?? throw new ArgumentNullException(nameof(data));
            this.length = length;
        }

        public bool IsValidSize()
        {
            const int MinPacketSize = 1;

            if (length < MinPacketSize)
                return false;


            const int MinCommandSize = 2;
            const int MinNotifySize = 1 + 2 + 2 + 4;
            // Min size of message given to Mirage
            const int MinUnreliableSize = 3;

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
    }
}
