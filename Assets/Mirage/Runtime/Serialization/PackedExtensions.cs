using System;
using System.Runtime.CompilerServices;

namespace Mirage.Serialization
{
    /// <remarks>
    /// <see href="https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba">zigzag encoding</see><br/>
    /// <see href="http://sqlite.org/src4/doc/trunk/www/varint.wiki">Variable-Length Integers</see><br/>
    /// </remarks>
    public static class PackedExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WritePackedInt32(this NetworkWriter writer, int i)
        {
            var zigzagged = ZigZag.Encode(i);
            writer.WritePackedUInt32(zigzagged);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WritePackedUInt32(this NetworkWriter writer, uint value)
        {
            // we can use uint64 here because it will be same bits once packed
            writer.WritePackedUInt64(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WritePackedInt64(this NetworkWriter writer, long i)
        {
            var zigzagged = ZigZag.Encode(i);
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




        public static int ReadPackedInt32(this NetworkReader reader)
        {
            var data = reader.ReadPackedUInt32();
            return ZigZag.Decode(data);
        }

        /// <exception cref="OverflowException">throws if values overflows uint</exception>
        public static uint ReadPackedUInt32(this NetworkReader reader) => checked((uint)reader.ReadPackedUInt64());

        public static long ReadPackedInt64(this NetworkReader reader)
        {
            var data = reader.ReadPackedUInt64();
            return ZigZag.Decode(data);
        }

        public static ulong ReadPackedUInt64(this NetworkReader reader)
        {
            var a0 = reader.ReadByte();
            if (a0 < 241)
            {
                return a0;
            }

            var a1 = reader.ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + ((a0 - (ulong)241) << 8) + a1;
            }

            var a2 = reader.ReadByte();
            if (a0 == 249)
            {
                return 2288 + ((ulong)a1 << 8) + a2;
            }

            var a3 = reader.ReadByte();
            if (a0 == 250)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16);
            }

            var a4 = reader.ReadByte();
            if (a0 == 251)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24);
            }

            var a5 = reader.ReadByte();
            if (a0 == 252)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32);
            }

            var a6 = reader.ReadByte();
            if (a0 == 253)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40);
            }

            var a7 = reader.ReadByte();
            if (a0 == 254)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48);
            }

            var a8 = reader.ReadByte();
            if (a0 == 255)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48) + (((ulong)a8) << 56);
            }

            throw new DataMisalignedException("ReadPackedUInt64() failure: " + a0);
        }
    }
}
