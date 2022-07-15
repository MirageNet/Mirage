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
            if (buffer == null)
            {
                WriteCountPlusOne(writer, null);
                return;
            }

            WriteCountPlusOne(writer, count);
            writer.WriteBytes(buffer, offset, count);
        }

        /// <summary>
        /// Write method for weaver to use
        /// </summary>
        /// <param name="buffer">array or null</param>
        public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer)
        {
            WriteCountPlusOne(writer, buffer?.Length);

            if (buffer == null)
                return;

            writer.WriteBytes(buffer, 0, buffer.Length);
        }

        public static void WriteBytesAndSizeSegment(this NetworkWriter writer, ArraySegment<byte> buffer)
        {
            writer.WriteBytesAndSize(buffer.Array, buffer.Offset, buffer.Count);
        }

        public static void WriteList<T>(this NetworkWriter writer, List<T> list)
        {
            WriteCountPlusOne(writer, list?.Count);

            if (list is null)
                return;

            var length = list.Count;
            for (var i = 0; i < length; i++)
                writer.Write(list[i]);
        }

        public static void WriteArray<T>(this NetworkWriter writer, T[] array)
        {
            WriteCountPlusOne(writer, array?.Length);

            if (array is null)
                return;

            var length = array.Length;
            for (var i = 0; i < length; i++)
                writer.Write(array[i]);
        }

        public static void WriteArraySegment<T>(this NetworkWriter writer, ArraySegment<T> segment)
        {
            var array = segment.Array;

            if (array == null)
            {
                WriteCountPlusOne(writer, null);
            }
            else
            {
                // cache these properties in local variable because they wont change and calling properties has performance cost
                var offset = segment.Offset;
                var length = segment.Count;

                WriteCountPlusOne(writer, length);
                for (var i = 0; i < length; i++)
                {
                    writer.Write(array[offset + i]);
                }
            }
        }


        /// <returns>array or null</returns>
        public static byte[] ReadBytesAndSize(this NetworkReader reader)
        {
            // dont need to ValidateSize here because ReadBytes does it

            return ReadCountPlusOne(reader, out var count)
                ? reader.ReadBytes(count)
                : null;
        }

        public static ArraySegment<byte> ReadBytesAndSizeSegment(this NetworkReader reader)
        {
            // dont need to ValidateSize here because we dont allocate for segment

            return ReadCountPlusOne(reader, out var count)
                ? reader.ReadBytesSegment(count)
                : default;
        }

        public static byte[] ReadBytes(this NetworkReader reader, int count)
        {
            // we know each element is 8 bits, so count*8 for max size
            ValidateSize(reader, count * 8);

            var bytes = new byte[count];
            reader.ReadBytes(bytes, 0, count);
            return bytes;
        }

        public static List<T> ReadList<T>(this NetworkReader reader)
        {
            var hasValue = ReadCountPlusOne(reader, out var length);
            if (!hasValue)
                return null;

            ValidateSize(reader, length);

            var result = new List<T>(length);
            for (var i = 0; i < length; i++)
            {
                result.Add(reader.Read<T>());
            }
            return result;
        }

        public static T[] ReadArray<T>(this NetworkReader reader)
        {
            var hasValue = ReadCountPlusOne(reader, out var length);
            if (!hasValue)
                return null;

            ValidateSize(reader, length);

            var result = new T[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = reader.Read<T>();
            }
            return result;
        }

        public static ArraySegment<T> ReadArraySegment<T>(this NetworkReader reader)
        {
            var array = reader.ReadArray<T>();
            return array != null ? new ArraySegment<T>(array) : default;
        }


        /// <summary>Writes null as 0, and all over values as +1</summary>
        /// <param name="count">The real count or null if collection is is null</param>
        internal static void WriteCountPlusOne(NetworkWriter writer, int? count)
        {
            // null is supported
            // write 0 for null array, increment normal size by 1 to save bandwith
            // (using size=-1 for null would limit max size to 32kb instead of 64kb)
            if (count.HasValue)
            {
                writer.WritePackedUInt32(checked((uint)count) + 1u);
            }
            else
            {
                writer.WritePackedUInt32(0);
            }
        }

        /// <summary>Reads 0 as null, and all over values as -1</summary>
        /// <param name="count">The real count of the </param>
        /// <returns>true if collection has value, false if collection is null</returns>
        internal static bool ReadCountPlusOne(NetworkReader reader, out int count)
        {
            // count = 0 means the array was null
            // otherwise count -1 is the length of the array
            var value = reader.ReadPackedUInt32();
            // Use checked() to force it to throw OverflowException if data is invalid/
            // do -1 after checked, incase value is 0 (count will be -1, but ok because we will return false in that case)
            count = checked((int)value) - 1;
            return value != 0;
        }

        /// <summary>
        /// Use to check max size in reader before allocating array/list
        /// <para>Assumes each element is only 1 bit, so max size allocated will be MTU*8 if attacks tries to attack</para>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="lengthInBits"></param>
        internal static void ValidateSize(NetworkReader reader, int lengthInBits)
        {
            // T might be only 1 bit long, so we have to check vs bit length
            // todo have weaver calculate minimum size for T, so we can use it here instead of 1 bit
            //     NOTE: we cant just use sizeof(T) because T might be bitpacked so smaller than size in memory
            if (!reader.CanReadBits(lengthInBits))
                throw new EndOfStreamException($"Can't read {lengthInBits} elements because it would read past the end of the stream.");
        }
    }
}
