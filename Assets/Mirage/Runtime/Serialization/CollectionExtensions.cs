using System;
using System.Collections.Generic;
using System.IO;

namespace Mirage.Serialization
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// For byte arrays with dynamic size, where the reader doesn't know how many will come 
        /// </summary>
        /// <param name="buffer">array or null</param>
        public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer, int offset, int count)
        {
            // null is supported
            // write 0 for null array, increment normal size by 1 to save bandwith
            // (using size=-1 for null would limit max size to 32kb instead of 64kb)
            if (buffer == null)
            {
                writer.WritePackedUInt32(0u);
                return;
            }
            writer.WritePackedUInt32(checked((uint)count) + 1u);
            writer.WriteBytes(buffer, offset, count);
        }

        /// <summary>
        /// Write method for weaver to use
        /// </summary>
        /// <param name="buffer">array or null</param>
        public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer)
        {
            // buffer might be null, so we can't use .Length in that case
            writer.WriteBytesAndSize(buffer, 0, buffer != null ? buffer.Length : 0);
        }

        public static void WriteBytesAndSizeSegment(this NetworkWriter writer, ArraySegment<byte> buffer)
        {
            writer.WriteBytesAndSize(buffer.Array, buffer.Offset, buffer.Count);
        }


        public static void WriteList<T>(this NetworkWriter writer, List<T> list)
        {
            if (list is null)
            {
                writer.WritePackedInt32(-1);
                return;
            }
            writer.WritePackedInt32(list.Count);
            for (int i = 0; i < list.Count; i++)
                writer.Write(list[i]);
        }

        public static void WriteArray<T>(this NetworkWriter writer, T[] array)
        {
            if (array is null)
            {
                writer.WritePackedInt32(-1);
                return;
            }
            writer.WritePackedInt32(array.Length);
            for (int i = 0; i < array.Length; i++)
                writer.Write(array[i]);
        }

        public static void WriteArraySegment<T>(this NetworkWriter writer, ArraySegment<T> segment)
        {
            int length = segment.Count;
            writer.WritePackedInt32(length);
            for (int i = 0; i < length; i++)
            {
                writer.Write(segment.Array[segment.Offset + i]);
            }
        }



        /// <returns>array or null</returns>
        public static byte[] ReadBytesAndSize(this NetworkReader reader)
        {
            // count = 0 means the array was null
            // otherwise count -1 is the length of the array
            uint count = reader.ReadPackedUInt32();

            // Use checked() to force it to throw OverflowException if data is invalid
            return count == 0 ? null : reader.ReadBytes(checked((int)(count - 1u)));
        }

        public static ArraySegment<byte> ReadBytesAndSizeSegment(this NetworkReader reader)
        {
            // count = 0 means the array was null
            // otherwise count - 1 is the length of the array
            uint count = reader.ReadPackedUInt32();
            return count == 0 ? default : reader.ReadBytesSegment(checked((int)(count - 1u)));
        }

        public static byte[] ReadBytes(this NetworkReader reader, int count)
        {
            byte[] bytes = new byte[count];
            reader.ReadBytes(bytes, 0, count);
            return bytes;
        }

        public static List<T> ReadList<T>(this NetworkReader reader)
        {
            int length = reader.ReadPackedInt32();
            if (length < 0)
                return null;
            var result = new List<T>(length);
            for (int i = 0; i < length; i++)
            {
                result.Add(reader.Read<T>());
            }
            return result;
        }

        public static T[] ReadArray<T>(this NetworkReader reader)
        {
            int length = reader.ReadPackedInt32();
            if (length < 0)
                return null;
            // T might be only 1 bit long, so we have to check vs bit length
            // todo have weaver calculate minimum size for T, so we can use it here instead of 1 bit
            //     NOTE: we cant just use sizeof(T) because T might be bitpacked so smaller than size in memory
            if (!reader.CanReadBits(length))
                throw new EndOfStreamException($"Can't read {length} elements because it would read past the end of the stream.");
            var result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = reader.Read<T>();
            }
            return result;
        }
    }
}
