namespace Mirage.Serialization
{
    /// <remarks>
    /// <see href="https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba">zigzag encoding</see><br/>
    /// <see href="http://sqlite.org/src4/doc/trunk/www/varint.wiki">Variable-Length Integers</see><br/>
    /// </remarks>
    public static class PackedExtensions
    {
        public static void WritePackedInt32(this NetworkWriter writer, int i)
        {
            uint zigzagged = (uint)((i >> 31) ^ (i << 1));
            writer.WritePackedUInt32(zigzagged);
        }

        public static void WritePackedUInt32(this NetworkWriter writer, uint value)
        {
            // for 32 bit values WritePackedUInt64 writes the
            // same exact thing bit by bit
            writer.WritePackedUInt64(value);
        }

        public static void WritePackedInt64(this NetworkWriter writer, long i)
        {
            ulong zigzagged = (ulong)((i >> 63) ^ (i << 1));
            writer.WritePackedUInt64(zigzagged);
        }

        public static void WritePackedUInt64(this NetworkWriter writer, ulong value)
        {
            // numbers are usually small,  have special encoding for smaller numbers
            if (value <= 240)
            {
                writer.WriteByte((byte)value);
                return;
            }
            if (value <= 2287)
            {
                writer.WriteByte((byte)(((value - 240) >> 8) + 241));
                writer.WriteByte((byte)(value - 240));
                return;
            }
            if (value <= 67823)
            {
                writer.WriteByte(249);
                writer.WriteByte((byte)((value - 2288) >> 8));
                writer.WriteByte((byte)(value - 2288));
                return;
            }

            // first byte determines how many bytes 250 => 3 bytes, 251 => 4 bytes
            // etc...
            if (value <= 0xffffff)
                writer.WriteByte(250);
            else if (value <= 0xffffffff)
                writer.WriteByte(251);
            else if (value <= 0xffffffffff)
                writer.WriteByte(252);
            else if (value <= 0xffffffffffff)
                writer.WriteByte(253);
            else if (value <= 0xffffffffffffff)
                writer.WriteByte(254);
            else writer.WriteByte(255);

            // write the data
            while (value > 0)
            {
                writer.WriteByte((byte)value);
                value >>= 8;
            }
        }
    }
}
