using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Received packet
    /// <para>contains raw data and helper methods to help process that data</para>
    /// </summary>
    internal ref struct Packet
    {
        public readonly ReadOnlySpan<byte> Span;

        public Packet(ReadOnlySpan<byte> span)
        {
            Span = span;
        }

        public PacketType Type => (PacketType)Span[0];
        public Commands Command => (Commands)Span[1];
        public int Length => Span.Length;
    }
}
