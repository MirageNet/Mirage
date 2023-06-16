using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Holds a collection of <see cref="ByteBuffer"/> so they can be re-used without allocations
    /// </summary>
    public class Pool<T> where T : class
    {
        private const int POOL_EMPTY = -1;

        private readonly ILogger _logger;
        private readonly CreateNewItem _createNew;
        private readonly int _bufferSize;
        private T[] _pool;
        private int _maxPoolSize;
        private int _next = -1;
        private int _created = 0;

        private OverMaxLog _overMaxLog = new OverMaxLog();

        /// <summary>
        /// sets max pool size and then creates writers up to new start size
        /// </summary>
        /// <param name="startPoolSize"></param>
        /// <param name="maxPoolSize"></param>
        public void Configure(int startPoolSize, int maxPoolSize)
        {
            if (startPoolSize > maxPoolSize) throw new ArgumentException("Start Size must be less than max size", nameof(startPoolSize));

            if (_maxPoolSize != maxPoolSize)
            {
                _maxPoolSize = maxPoolSize;
                Array.Resize(ref _pool, maxPoolSize);
            }

            for (var i = _created; i < startPoolSize; i++)
            {
                Put(CreateNewBuffer());
            }

            if (_logger.Enabled(LogType.Log)) _logger.Log(LogType.Log, $"Configuring buffer, start Size {startPoolSize}, max size {maxPoolSize}");
        }

        /// <summary>
        /// Creates pool, that does not require Buffer size
        /// </summary>
        /// <param name="bufferSize">size of each buffer</param>
        /// <param name="startPoolSize">how many buffers to create at start</param>
        /// <param name="maxPoolSize">max number of buffers in pool</param>
        /// <param name="logger"></param>
        public Pool(CreateNewItemNoCount createNew, int startPoolSize, int maxPoolSize, ILogger logger = null)
            : this((_, p) => createNew.Invoke(p), default, startPoolSize, maxPoolSize, logger) { }

        /// <summary>
        /// Creates pool where buffer size will be passed to items when created them
        /// </summary>
        /// <param name="bufferSize">size of each buffer</param>
        /// <param name="startPoolSize">how many buffers to create at start</param>
        /// <param name="maxPoolSize">max number of buffers in pool</param>
        /// <param name="logger"></param>
        public Pool(CreateNewItem createNew, int bufferSize, int startPoolSize, int maxPoolSize, ILogger logger = null)
        {
            if (startPoolSize > maxPoolSize) throw new ArgumentException("Start size must be less than max size", nameof(startPoolSize));
            _createNew = createNew ?? throw new ArgumentNullException(nameof(createNew));

            _bufferSize = bufferSize;
            _maxPoolSize = maxPoolSize;
            _logger = logger;

            _pool = new T[maxPoolSize];
            for (var i = 0; i < startPoolSize; i++)
            {
                Put(CreateNewBuffer());
            }
        }


        private T CreateNewBuffer()
        {
            _created++;
            _overMaxLog.CheckLimit(this);
            return _createNew.Invoke(_bufferSize, this);
        }

        public T Take()
        {
            if (_next == POOL_EMPTY)
            {
                return CreateNewBuffer();
            }
            else
            {
                // todo is it a security risk to not clear buffer?

                // take then decrement
                var item = _pool[_next];
                _pool[_next] = null;
                _next--;
                return item;
            }
        }

        public void Put(T buffer)
        {
            if (_next < _maxPoolSize - 1)
            {
                // increment then put
                _pool[++_next] = buffer;
            }
            else
            {
                // buffer is left for GC, so decrement created
                _created--;
            }
        }

        public delegate T CreateNewItemNoCount(Pool<T> pool);
        public delegate T CreateNewItem(int bufferSize, Pool<T> pool);

        private struct OverMaxLog
        {
            // 10 seconds log interval
            private const float LOG_INTERVAL = 10;

            private float _nextLogTime;
            private int _lastLogValue;

            private float GetTime()
            {
                return Stopwatch.GetTimestamp() / (float)Stopwatch.Frequency;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CheckLimit(Pool<T> pool)
            {
                if (pool._created >= pool._maxPoolSize && pool._logger.Enabled(LogType.Warning))
                {
                    var now = GetTime();

                    // if has been enough time since last log, then log again 
                    if (now > _nextLogTime)
                    {
                        _lastLogValue = pool._created;
                        _nextLogTime = now + LOG_INTERVAL;
                        pool._logger.Log(LogType.Warning, $"Pool Max Size reached, type:{typeof(T).Name} created:{pool._created + 1} max:{pool._maxPoolSize}");
                        return;
                    }

                    // if pool has grown enough since last log (but has been less than LogInterval) then log again now
                    if (pool._created > _lastLogValue + pool._maxPoolSize)
                    {
                        _lastLogValue = pool._created;
                        _nextLogTime = now + LOG_INTERVAL;
                        pool._logger.Log(LogType.Warning, $"Pool Max Size reached, type:{typeof(T).Name} created:{pool._created + 1} max:{pool._maxPoolSize}");
                    }
                }
            }
        }
    }
}
