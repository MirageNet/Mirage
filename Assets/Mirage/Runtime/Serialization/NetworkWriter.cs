using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Serialization
{
    /// <summary>
    /// a class that holds writers for the different types
    /// Note that c# creates a different static variable for each
    /// type
    /// This will be populated by the weaver
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Writer<T>
    {
        public static Action<NetworkWriter, T> Write { internal get; set; }
    }

    /// <summary>
    /// Binary stream Writer. Supports simple types, buffers, arrays, structs, and nested types
    /// <para>Use <see cref="NetworkWriterPool.GetWriter">NetworkWriter.GetWriter</see> to reduce memory allocation</para>
    /// </summary>
    public class NetworkWriter
    {
        public const int MaxStringLength = 1024 * 32;

        // create writer immediately with it's own buffer so no one can mess with it and so that we can resize it.
        // note: BinaryWriter allocates too much, so we only use a MemoryStream
        // => 1500 bytes by default because on average, most packets will be <= MTU
        byte[] buffer = new byte[1500];

        // 'int' is the best type for .Position. 'short' is too small if we send >32kb which would result in negative .Position
        // -> converting long to int is fine until 2GB of data (MAX_INT), so we don't have to worry about overflows here
        int position;

        public int Length { get; private set; }

        public int Position
        {
            get => position;
            set
            {
                position = value;
                EnsureLength(value);
            }
        }

        /// <summary>
        /// Reset both the position and length of the stream
        /// </summary>
        /// <remarks>
        /// Leaves the capacity the same so that we can reuse this writer without extra allocations
        /// </remarks>
        public void Reset()
        {
            position = 0;
            Length = 0;
        }

        /// <summary>
        /// Sets length, moves position if it is greater than new length
        /// </summary>
        /// <param name="newLength"></param>
        /// <remarks>
        /// Zeros out any extra length created by setlength
        /// </remarks>
        public void SetLength(int newLength)
        {
            int oldLength = Length;

            // ensure length & capacity
            EnsureLength(newLength);

            // zero out new length
            if (oldLength < newLength)
            {
                Array.Clear(buffer, oldLength, newLength - oldLength);
            }

            Length = newLength;
            position = Mathf.Min(position, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureLength(int value)
        {
            if (Length < value)
            {
                Length = value;
                EnsureCapacity(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureCapacity(int value)
        {
            if (buffer.Length < value)
            {
                int capacity = Math.Max(value, buffer.Length * 2);
                Array.Resize(ref buffer, capacity);
            }
        }

        // MemoryStream has 3 values: Position, Length and Capacity.
        // Position is used to indicate where we are writing
        // Length is how much data we have written
        // capacity is how much memory we have allocated
        // ToArray returns all the data we have written,  regardless of the current position
        public byte[] ToArray()
        {
            byte[] data = new byte[Length];
            Array.ConstrainedCopy(buffer, 0, data, 0, Length);
            return data;
        }

        // Gets the serialized data in an ArraySegment<byte>
        // this is similar to ToArray(),  but it gets the data in O(1)
        // and without allocations.
        // Do not write anything else or modify the NetworkWriter
        // while you are using the ArraySegment
        public ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(buffer, 0, Length);
        }

        public void WriteByte(byte value)
        {
            EnsureLength(position + 1);
            buffer[position++] = value;
        }


        // for byte arrays with consistent size, where the reader knows how many to read
        // (like a packet opcode that's always the same)
        public void WriteBytes(byte[] buffer, int offset, int count)
        {
            EnsureLength(position + count);
            Array.ConstrainedCopy(buffer, offset, this.buffer, position, count);
            position += count;
        }

        public void WriteUInt32(uint value)
        {
            EnsureLength(position + 4);
            buffer[position++] = (byte)value;
            buffer[position++] = (byte)(value >> 8);
            buffer[position++] = (byte)(value >> 16);
            buffer[position++] = (byte)(value >> 24);
        }

        public void WriteInt32(int value) => WriteUInt32((uint)value);

        public void WriteUInt64(ulong value)
        {
            EnsureLength(position + 8);
            buffer[position++] = (byte)value;
            buffer[position++] = (byte)(value >> 8);
            buffer[position++] = (byte)(value >> 16);
            buffer[position++] = (byte)(value >> 24);
            buffer[position++] = (byte)(value >> 32);
            buffer[position++] = (byte)(value >> 40);
            buffer[position++] = (byte)(value >> 48);
            buffer[position++] = (byte)(value >> 56);
        }

        public void WriteInt64(long value) => WriteUInt64((ulong)value);

        /// <summary>
        /// Writes any type that mirror supports
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void Write<T>(T value)
        {
            if (Writer<T>.Write == null)
                Debug.AssertFormat(
                    Writer<T>.Write != null,
                    @"No writer found for {0}. See https://miragenet.github.io/Mirage/Articles/General/Troubleshooting.html for details",
                    typeof(T));

            Writer<T>.Write(this, value);
        }
    }
}
