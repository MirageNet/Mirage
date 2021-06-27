namespace Mirage.SocketLayer
{
    public static class ByteUtils
    {
        public static void WriteByte(byte[] buffer, ref int offset, byte value)
        {
            buffer[offset] = value;
            offset++;
        }

        public static byte ReadByte(byte[] buffer, ref int offset)
        {
            byte a = buffer[offset];
            offset++;

            return a;
        }


        public static void WriteUShort(byte[] buffer, ref int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            offset++;
            buffer[offset] = (byte)(value >> 8);
            offset++;
        }

        public static ushort ReadUShort(byte[] buffer, ref int offset)
        {
            ushort a = buffer[offset];
            offset++;
            ushort b = buffer[offset];
            offset++;

            return (ushort)(a | (b << 8));
        }


        public static void WriteUInt(byte[] buffer, ref int offset, uint value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
            offset += 4;
        }

        public static uint ReadUInt(byte[] buffer, ref int offset)
        {
            uint a = buffer[offset];
            uint b = buffer[offset + 1];
            uint c = buffer[offset + 2];
            uint d = buffer[offset + 3];
            offset += 4;

            return a | (b << 8) | (c << 16) | (d << 24);
        }

        public static void WriteULong(byte[] buffer, ref int offset, ulong value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);

            buffer[offset + 4] = (byte)(value >> 32);
            buffer[offset + 5] = (byte)(value >> 40);
            buffer[offset + 6] = (byte)(value >> 48);
            buffer[offset + 7] = (byte)(value >> 56);
            offset += 8;
        }

        public static ulong ReadULong(byte[] buffer, ref int offset)
        {
            ulong a = buffer[offset];
            ulong b = buffer[offset + 1];
            ulong c = buffer[offset + 2];
            ulong d = buffer[offset + 3];
            ulong e = buffer[offset + 4];
            ulong f = buffer[offset + 5];
            ulong g = buffer[offset + 6];
            ulong h = buffer[offset + 7];
            offset += 8;

            return a | (b << 8) | (c << 16) | (d << 24) | (e << 32) | (f << 40) | (g << 48) | (h << 56);
        }
    }
}
