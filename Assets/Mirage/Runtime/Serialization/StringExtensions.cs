using System;
using System.IO;
using System.Text;

namespace Mirage.Serialization
{
    public static class StringExtensions
    {
        private static int maxStringLength = 1300;

        /// <summary>
        /// Maximum number of bytes a string can be serialized to. This is to avoid allocation attack.
        /// <para>Defaults MTU, 1300</para>
        /// <para>NOTE: this is byte size after Encoding</para>
        /// <para>IMPORTANT: Setting this property will resize the internal buffer. Do not call in hotpath. It is best to call once when you start the application</para>
        /// </summary>
        public static int MaxStringLength
        {
            get => maxStringLength;
            set
            {
                if (maxStringLength == value)
                    return;

                maxStringLength = value;
                Array.Resize(ref stringBuffer, value);
            }
        }

        public static readonly UTF8Encoding defaultEncoding = new UTF8Encoding(false, true);
        private static byte[] stringBuffer = new byte[MaxStringLength];

        /// <param name="value">string or null</param>
        public static void WriteString(this NetworkWriter writer, string value)
        {
            // note, only use StringStore for default encoding
            if (writer.StringStore != null)
            {
                writer.StringStore.WriteString(writer, value);
                return;
            }

            WriteString(writer, value, defaultEncoding);
        }

        /// <returns>string or null</returns>
        /// <exception cref="ArgumentException">Throws if invalid utf8 string is received</exception>
        public static string ReadString(this NetworkReader reader)
        {
            if (reader.StringStore != null)
            {
                return reader.StringStore.ReadString(reader);
            }

            return ReadString(reader, defaultEncoding);
        }

        /// <param name="encoding">Use this for encoding other than the default (UTF8). Make sure to use same encoding for ReadString</param>
        /// <param name="value">string or null</param>
        public static void WriteString(this NetworkWriter writer, string value, Encoding encoding)
        {
            // write 0 for null support, increment real size by 1
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

        /// <param name="encoding">Use this for encoding other than the default (UTF8). Make sure to use same encoding for WriterString</param>
        /// <returns>string or null</returns>
        /// <exception cref="ArgumentException">Throws if invalid utf8 string is received</exception>
        public static string ReadString(this NetworkReader reader, Encoding encoding)
        {
            // read number of bytes
            var sizePlusOne = reader.ReadUInt16();

            if (sizePlusOne == 0)
                return null;

            var size = sizePlusOne - 1;

            // make sure it's within limits to avoid allocation attacks etc.
            if (size >= MaxStringLength)
            {
                throw new EndOfStreamException($"ReadString too long: {size}. Limit is: {MaxStringLength}");
            }

            var data = reader.ReadBytesSegment(size);

            // convert directly from buffer to string via encoding
            return encoding.GetString(data.Array, data.Offset, data.Count);
        }

        public static void WriteString(this NetworkWriter writer, string value, int maxLength)
        {
            if (value != null && value.Length > maxLength)
                throw new SerializationLimitException($"String length {value.Length} exceeds maximum limit of {maxLength}");

            WriteString(writer, value);
        }

        public static void WriteString(this NetworkWriter writer, string value, int maxLength, Encoding encoding)
        {
            if (value != null && value.Length > maxLength)
                throw new SerializationLimitException($"String length {value.Length} exceeds maximum limit of {maxLength}");

            WriteString(writer, value, encoding);
        }

        public static string ReadString(this NetworkReader reader, int maxLength)
        {
            if (reader.StringStore != null)
            {
                var result = reader.StringStore.ReadString(reader);
                if (result != null && result.Length > maxLength)
                    throw new SerializationLimitException($"String exceeds maximum length: {result.Length}. Limit: {maxLength}");

                return result;
            }
            else
            {
                return ReadString(reader, maxLength, defaultEncoding);
            }
        }

        public static string ReadString(this NetworkReader reader, int maxLength, Encoding encoding)
        {
            // read number of bytes
            var sizePlusOne = reader.ReadUInt16();
            if (sizePlusOne == 0)
                return null;

            var size = sizePlusOne - 1;

            // first checks if incoming bytes realSize > max byte count (fast fail early)
            if (size > encoding.GetMaxByteCount(maxLength))
                throw new SerializationLimitException($"ReadString byte size {size} is larger than maximum possible for character limit {maxLength}");

            // make sure it's within limits to avoid allocation attacks etc.
            if (size >= MaxStringLength)
                throw new EndOfStreamException($"ReadString too long: {size}. Limit is: {MaxStringLength}");

            var data = reader.ReadBytesSegment(size);

            // check character count to avoid allocating a string that exceeds the limit
            var charCount = encoding.GetCharCount(data.Array, data.Offset, data.Count);
            if (charCount > maxLength)
                throw new SerializationLimitException($"ReadString character count {charCount} exceeds limit of {maxLength}");

            // convert directly from buffer to string via encoding
            return encoding.GetString(data.Array, data.Offset, data.Count);
        }
    }
}
