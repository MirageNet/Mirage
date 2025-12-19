using System;
using System.Runtime.CompilerServices;
using ILogger = UnityEngine.ILogger;

namespace Mirage.SocketLayer
{
    public class RingBuffer<T>
    {
        public readonly struct Option
        {
            public readonly bool HasValue;
            public readonly T Value;

            private Option(bool hasValue, T value)
            {
                HasValue = hasValue;
                Value = value;
            }

            public static Option Some(T value) => new Option(true, value);
            public static Option None() => new Option(false, default);
        }

        public readonly Sequencer Sequencer;
        private readonly ILogger _logger;


        private readonly Option[] _buffer;

        /// <summary>oldest item</summary>
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

        public int Capacity => _buffer.Length;

        public RingBuffer(int bitCount, ILogger logger)
        {
            Sequencer = new Sequencer(bitCount);
            _buffer = new Option[1 << bitCount];
            _logger = logger;
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
        /// <returns>sequence of written item</returns>
        public uint Enqueue(T item)
        {
            var distance = Sequencer.Distance(_write, _read);
            if (distance == -1)
                throw new BufferFullException($"Buffer is full, write:{_write} read:{_read}");

            _buffer[_write] = Option.Some(item);
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
            return TryGet(_read, out item);
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
            return _buffer[inBounds].HasValue;
        }

        /// <summary>
        /// Removes the item at read index and increments read index
        /// <para>can be used after <see cref="TryPeak"/> to do the same as <see cref="TryDequeue"/></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveNext()
        {
            _logger?.DebugAssert(_buffer[_read].HasValue, "Removing item, but it was already null");
            _buffer[_read] = Option.None();
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
            return item.Value;
        }

        /// <summary>
        /// Tries to remove the item at read index
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if item exists, or false if it is missing</returns>
        public bool TryDequeue(out T item)
        {
            if (TryGet(_read, out item))
            {
                RemoveNext();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGet(uint index, out T item)
        {
            var entry = _buffer[index];
            item = entry.Value;
            return entry.HasValue;
        }

        public void InsertAt(uint index, T item)
        {
            _logger?.DebugAssert(!_buffer[index].HasValue, "Insert item, already had a value");
            _count++;
            _buffer[index] = Option.Some(item);
        }
        public void RemoveAt(uint index)
        {
            _logger?.DebugAssert(_buffer[index].HasValue, "Removing item, but it was already null");
            _count--;
            _buffer[index] = Option.None();
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
            while (_write != _read && !_buffer[_read].HasValue)
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

        public void ClearAndRelease(Action<T> releaseItem)
        {
            for (var i = 0; i < _buffer.Length; i++)
            {
                var item = _buffer[i];

                // note: releaseItem might remove items from buffer
                // this is find because we are looking over every item, and resetting values at the end
                if (item.HasValue)
                    releaseItem?.Invoke(item.Value);

                _buffer[i] = Option.None();
            }

            _count = 0;
            _read = 0;
            _write = 0;
        }
    }
}
