using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Recieved packet
    /// <para>contains raw data and helper methods to help process that data</para>
    /// </summary>
    internal struct Packet
    {
        const int MinPacketSize = 1;
        const int MinCommandSize = 2;
        /// <summary>
        /// Min size of message given to Mirage
        /// </summary>
        const int MinMessageSize = 3;

        public byte[] data;
        public int length;

        public Packet(byte[] data, int length)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
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
                case PacketType.Notify:
                    return length >= MinMessageSize;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }

        public PacketType type => (PacketType)data[0];
        public Commands command => (Commands)data[1];

        public ArraySegment<byte> ToSegment()
        {
            // ingore packet type
            return new ArraySegment<byte>(data, 1, length);
        }
    }
}
