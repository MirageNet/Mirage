using System;
using System.IO;
using System.Text;

namespace Mirage.Serialization
{
    public static class StringExtensions
    {
        /// <summary>
        /// Defaults MTU, 1300
        /// <para>Can be changed by user if they need to</para>
        /// </summary>
        public static int MaxStringLength = 1300;
        private static readonly UTF8Encoding encoding = new UTF8Encoding(false, true);
        private static readonly byte[] stringBuffer = new byte[MaxStringLength];

        /// <param name="value">string or null</param>
        public static void WriteString(this NetworkWriter writer, string value)
        {
            // write 0 for null support, increment real size by 1
            // (note: original HLAPI would write "" for null strings, but if a
            //        string is null on the server then it should also be null
            //        on the client)
            if (value == null)
            {
                writer.WriteUInt16(0);
                return;
            }

            // write string with same method as NetworkReader
            // convert to byte[]
            var size = encoding.GetBytes(value, 0, value.Length, stringBuffer, 0);

            // check if within max size
            if (size >= MaxStringLength)
            {
                throw new DataMisalignedException($"NetworkWriter.Write(string) too long: {size}. Limit: {MaxStringLength}");
            }

            // write size and bytes
            writer.WriteUInt16(checked((ushort)(size + 1)));
            writer.WriteBytes(stringBuffer, 0, size);
        }


        /// <returns>string or null</returns>
        /// <exception cref="ArgumentException">Throws if invalid utf8 string is received</exception>
        public static string ReadString(this NetworkReader reader)
        {
            // read number of bytes
            var size = reader.ReadUInt16();

            if (size == 0)
                return null;

            var realSize = size - 1;

            // make sure it's within limits to avoid allocation attacks etc.
            if (realSize >= MaxStringLength)
            {
                throw new EndOfStreamException($"ReadString too long: {realSize}. Limit is: {MaxStringLength}");
            }

            var data = reader.ReadBytesSegment(realSize);

            // convert directly from buffer to string via encoding
            return encoding.GetString(data.Array, data.Offset, data.Count);
        }
    }
}
