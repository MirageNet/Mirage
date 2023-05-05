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

        public PacketType Type => (PacketType)Buffer.array[0];
        public Commands Command => (Commands)Buffer.array[1];
    }
}
