using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Received packet
    /// <para>contains raw data and helper methods to help process that data</para>
    /// </summary>
    internal struct Packet
    {
        public readonly ByteBuffer Buffer;
        public readonly int Length;

        public Packet(ByteBuffer data, int length)
        {
            Buffer = data ?? throw new ArgumentNullException(nameof(data));
            Length = length;
        }

        public bool IsValidSize()
        {
            const int minPacketSize = 1;

            if (Length < minPacketSize)
                return false;

            // Min size of message given to Mirage
            const int minMessageSize = 2;

            const int minCommandSize = 2;
            const int minUnreliableSize = 1 + minMessageSize;

            switch (Type)
            {
                case PacketType.Command:
                    return Length >= minCommandSize;

                case PacketType.Unreliable:
                    return Length >= minUnreliableSize;

                case PacketType.Notify:
                    return Length >= AckSystem.NOTIFY_HEADER_SIZE + minMessageSize;
                case PacketType.Reliable:
                    return Length >= AckSystem.MIN_RELIABLE_HEADER_SIZE + minMessageSize;
                case PacketType.ReliableFragment:
                    return Length >= AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE + 1;
                case PacketType.Ack:
                    return Length >= AckSystem.ACK_HEADER_SIZE;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }

        public PacketType Type => (PacketType)Buffer.array[0];
        public Commands Command => (Commands)Buffer.array[1];
    }
}
