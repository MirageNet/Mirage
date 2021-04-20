namespace Mirage.SocketLayer
{
    internal static class ByteUtils
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
            offset++;
            buffer[offset] = (byte)(value >> 8);
            offset++;
            buffer[offset] = (byte)(value >> 16);
            offset++;
            buffer[offset] = (byte)(value >> 24);
            offset++;
        }

        public static uint ReadUInt(byte[] buffer, ref int offset)
        {
            uint a = buffer[offset];
            offset++;
            uint b = buffer[offset];
            offset++;
            uint c = buffer[offset];
            offset++;
            uint d = buffer[offset];
            offset++;

            return a | (b << 8) | (c << 16) | (d << 24);
        }
    }
}
