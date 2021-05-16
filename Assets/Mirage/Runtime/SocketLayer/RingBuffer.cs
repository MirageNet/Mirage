using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirage.SocketLayer
{
    public class RingBuffer<T>
    {
        public readonly Sequencer Sequencer;

        readonly IEqualityComparer<T> comparer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsDefault(T value)
        {
            return comparer.Equals(value, default(T));
        }

        T[] buffer;
        /// <summary>oldtest item</summary>
        uint read;
        /// <summary>newest item</summary>
        uint write;

        /// <summary>manually keep track of number of items queued/inserted, this will be different from read to write range if removing/inserting not in order</summary>
        int count;

        public uint Read => read;
        public uint Write => write;

        /// <summary>
        /// Number of non-null items in buffer
        /// </summary>
        public int Count => count;

        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => buffer[index];
        }

        public RingBuffer(int bitCount) : this(bitCount, EqualityComparer<T>.Default) { }
        public RingBuffer(int bitCount, IEqualityComparer<T> comparer)
        {
            Sequencer = new Sequencer(bitCount);
            buffer = new T[1 << bitCount];
            this.comparer = comparer;
        }

        public bool IsFull => Sequencer.Distance(write, read) == -1;
        public long DistanceToRead(uint from)
        {
            return Sequencer.Distance(from, read);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>sequance of written item</returns>
        public uint Enqueue(T item)
        {
            long dist = Sequencer.Distance(write, read);
            if (dist == -1) { throw new InvalidOperationException($"Buffer is full, write:{write} read:{read}"); }

            buffer[write] = item;
            uint sequence = write;
            write = (uint)Sequencer.NextAfter(write);
            count++;
            return sequence;
        }
        public bool TryDequeue(out T item)
        {
            item = buffer[read];
            if (!IsDefault(item))
            {
                buffer[read] = default;
                read = (uint)Sequencer.NextAfter(read);
                count--;

                return true;
            }
            else
            {
                return false;
            }
        }

        public void InsertAt(uint index, T item)
        {
            count++;
            buffer[index] = item;
        }
        public void RemoveAt(uint index)
        {
            count--;
            buffer[index] = default;
        }


        /// <summary>
        /// Moves read index to next non empty position
        /// <para>this is useful when removing items from buffer in random order.</para>
        /// <para>Will stop when write == read, or when next buffer item is not empty</para>
        /// </summary>
        public void MoveReadToNextNonEmpty()
        {
            // if read == write, buffer is empty, dont move it
            // if buffer[read] is empty then read to next item
            while (write != read && IsDefault(buffer[read]))
            {
                read = (uint)Sequencer.NextAfter(read);
            }
        }

        /// <summary>
        /// Moves read 1 index
        /// </summary>
        public void MoveReadOne()
        {
            read = (uint)Sequencer.NextAfter(read);
        }
    }
}
