using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirage.SocketLayer
{
    public class RingBuffer<T>
    {
        public readonly Sequencer Sequencer;
        private readonly IEqualityComparer<T> _comparer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsDefault(T value)
        {
            return _comparer.Equals(value, default);
        }

        private readonly T[] _buffer;

        /// <summary>oldtest item</summary>
        private uint _read;

        /// <summary>newest item</summary>
        private uint _write;

        /// <summary>manually keep track of number of items queued/inserted, this will be different from read to write range if removing/inserting not in order</summary>
        private int _count;

        public uint Read => _read;
        public uint Write => _write;

        /// <summary>
        /// Number of non-null items in buffer
        /// <para>NOTE: this is not distance from read to write</para>
        /// </summary>
        public int Count => _count;

        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer[index];
        }
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer[index];
        }

        public RingBuffer(int bitCount) : this(bitCount, EqualityComparer<T>.Default) { }
        public RingBuffer(int bitCount, IEqualityComparer<T> comparer)
        {
            Sequencer = new Sequencer(bitCount);
            _buffer = new T[1 << bitCount];
            _comparer = comparer;
        }

        public bool IsFull => Sequencer.Distance(_write, _read) == -1;
        public long DistanceToRead(uint from)
        {
            return Sequencer.Distance(from, _read);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>sequance of written item</returns>
        public uint Enqueue(T item)
        {
            var dist = Sequencer.Distance(_write, _read);
            if (dist == -1) { throw new InvalidOperationException($"Buffer is full, write:{_write} read:{_read}"); }

            _buffer[_write] = item;
            var sequence = _write;
            _write = (uint)Sequencer.NextAfter(_write);
            _count++;
            return sequence;
        }

        /// <summary>
        /// Tries to read the item at read index
        /// <para>same as <see cref="TryDequeue"/> but does not remove the item after reading it</para>
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if item exists, or false if it is missing</returns>
        public bool TryPeak(out T item)
        {
            item = _buffer[_read];
            return !IsDefault(item);
        }

        /// <summary>
        /// Does item exist at index
        /// <para>Index will be moved into bounds</para>
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if item exists, or false if it is missing</returns>
        public bool Exists(uint index)
        {
            var inBounds = (uint)Sequencer.MoveInBounds(index);
            return !IsDefault(_buffer[inBounds]);
        }

        /// <summary>
        /// Removes the item at read index and increments read index
        /// <para>can be used after <see cref="TryPeak"/> to do the same as <see cref="TryDequeue"/></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveNext()
        {
            _buffer[_read] = default;
            _read = (uint)Sequencer.NextAfter(_read);
            _count--;
        }

        /// <summary>
        /// Removes next item and increments read index
        /// <para>Assumes next items exists, best to use this with <see cref="Exists"/></para>
        /// </summary>
        public T Dequeue()
        {
            var item = _buffer[_read];
            RemoveNext();
            return item;
        }

        /// <summary>
        /// Tries to remove the item at read index
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if item exists, or false if it is missing</returns>
        public bool TryDequeue(out T item)
        {
            item = _buffer[_read];
            if (!IsDefault(item))
            {
                RemoveNext();

                return true;
            }
            else
            {
                return false;
            }
        }


        public void InsertAt(uint index, T item)
        {
            _count++;
            _buffer[index] = item;
        }
        public void RemoveAt(uint index)
        {
            _count--;
            _buffer[index] = default;
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
            while (_write != _read && IsDefault(_buffer[_read]))
            {
                _read = (uint)Sequencer.NextAfter(_read);
            }
        }

        /// <summary>
        /// Moves read 1 index
        /// </summary>
        public void MoveReadOne()
        {
            _read = (uint)Sequencer.NextAfter(_read);
        }
    }
}
