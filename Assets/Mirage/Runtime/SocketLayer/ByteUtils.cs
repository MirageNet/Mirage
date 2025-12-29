using System;

namespace Mirage.SocketLayer
{
    public static class ByteUtils
    {
        public static void WriteByte(Span<byte> span, ref int offset, byte value)
        {
            span[offset] = value;
            offset++;
        }

        public static byte ReadByte(ReadOnlySpan<byte> span, ref int offset)
        {
            var a = span[offset];
            offset++;

            return a;
        }


        public static void WriteUShort(Span<byte> span, ref int offset, ushort value)
        {
            span[offset] = (byte)value;
            offset++;
            span[offset] = (byte)(value >> 8);
            offset++;
        }

        public static ushort ReadUShort(ReadOnlySpan<byte> span, ref int offset)
        {
            ushort a = span[offset];
            offset++;
            ushort b = span[offset];
            offset++;

            return (ushort)(a | (b << 8));
        }


        public static void WriteUInt(Span<byte> buffer, ref int offset, uint value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
            offset += 4;
        }

        public static uint ReadUInt(ReadOnlySpan<byte> span, ref int offset)
        {
            uint a = span[offset];
            uint b = span[offset + 1];
            uint c = span[offset + 2];
            uint d = span[offset + 3];
            offset += 4;

            return a | (b << 8) | (c << 16) | (d << 24);
        }

        public static void WriteULong(Span<byte> span, ref int offset, ulong value)
        {
            span[offset] = (byte)value;
            span[offset + 1] = (byte)(value >> 8);
            span[offset + 2] = (byte)(value >> 16);
            span[offset + 3] = (byte)(value >> 24);

            span[offset + 4] = (byte)(value >> 32);
            span[offset + 5] = (byte)(value >> 40);
            span[offset + 6] = (byte)(value >> 48);
            span[offset + 7] = (byte)(value >> 56);
            offset += 8;
        }

        public static ulong ReadULong(ReadOnlySpan<byte> span, ref int offset)
        {
            ulong a = span[offset];
            ulong b = span[offset + 1];
            ulong c = span[offset + 2];
            ulong d = span[offset + 3];
            ulong e = span[offset + 4];
            ulong f = span[offset + 5];
            ulong g = span[offset + 6];
            ulong h = span[offset + 7];
            offset += 8;

            return a | (b << 8) | (c << 16) | (d << 24) | (e << 32) | (f << 40) | (g << 48) | (h << 56);
        }
    }
}
