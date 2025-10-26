using System;
using System.Collections.Generic;
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

        private static readonly UTF8Encoding defaultEncoding = new UTF8Encoding(false, true);
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

        /// <param name="encoding">Use this for encoding other than the default (UTF8). Make sure to use same encoding for WriterString</param>
        /// <returns>string or null</returns>
        /// <exception cref="ArgumentException">Throws if invalid utf8 string is received</exception>
        public static string ReadString(this NetworkReader reader, Encoding encoding)
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
        public static void WriteStringStore(this NetworkWriter writer, StringStore store)
        {
            var count = (uint)store.Strings.Count;
            writer.WritePackedUInt32(count);
            for (var i = 0; i < count; i++)
                // use defaultEncoding, so we use the real write method and not the one that uses StringStore
                writer.WriteString(store.Strings[i], defaultEncoding);
        }
        public static StringStore ReadStringStore(this NetworkReader reader)
        {
            var store = new StringStore();
            var list = store.Strings;
            var count = reader.ReadPackedUInt32();
            for (var i = 0; i < count; i++)
                list.Add(reader.ReadString(defaultEncoding));
            return store;
        }
    }

    public class StringStore
    {
        public Dictionary<string, int> Lookup = new Dictionary<string, int>();
        public List<string> Strings = new List<string>();

        public int GetKey(string value)
        {
            if (Lookup.TryGetValue(value, out var index))
            {
                return index;
            }
            else
            {
                index = Strings.Count;
                Strings.Add(value);
                Lookup.Add(value, index);
                return index;
            }
        }

        public void WriteString(NetworkWriter writer, string value)
        {
            if (value == null)
            {
                writer.WritePackedUInt32(0);
            }
            else
            {
                var key = GetKey(value);
                writer.WritePackedUInt32(checked((uint)(key + 1)));
            }
        }

        public string ReadString(NetworkReader reader)
        {
            var key = reader.ReadPackedUInt32();
            if (key == 0)
                return null;

            var index = checked((int)(key - 1));
            return Strings[index];
        }
    }
}
