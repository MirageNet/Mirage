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

            // Min size of message given to Mirage
            const int MIN_MESSAGE_SIZE = 2;


            const int MIN_COMMAND_SIZE = 2;
            const int MIN_UNRELIABLE_SIZE = 1 + MIN_MESSAGE_SIZE;

            switch (type)
            {
                case PacketType.Command:
                    return length >= MIN_COMMAND_SIZE;

                case PacketType.Unreliable:
                    return length >= MIN_UNRELIABLE_SIZE;

                case PacketType.Notify:
                    return length >= AckSystem.NOTIFY_HEADER_SIZE + MIN_MESSAGE_SIZE;
                case PacketType.Reliable:
                    return length >= AckSystem.MIN_RELIABLE_HEADER_SIZE + MIN_MESSAGE_SIZE;
                case PacketType.ReliableFragment:
                    return length >= AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE + 1;
                case PacketType.Ack:
                    return length >= AckSystem.ACK_HEADER_SIZE;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }

        public PacketType type => (PacketType)buffer.array[0];
        public Commands command => (Commands)buffer.array[1];
    }
}
