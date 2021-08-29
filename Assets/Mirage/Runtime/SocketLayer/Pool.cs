using System;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Holds a collection of <see cref="ByteBuffer"/> so they can be re-used without allocations
    /// </summary>
    public class Pool<T> where T : class
    {
        const int PoolEmpty = -1;

        int maxPoolSize;
        readonly int bufferSize;
        readonly ILogger logger;
        public delegate T CreateNewItem(int bufferSize, Pool<T> pool);
        readonly CreateNewItem createNew;


        T[] pool;
        int next = -1;
        int created = 0;

        /// <summary>
        /// sets max pool size and then creates writers up to new start size
        /// </summary>
        /// <param name="startPoolSize"></param>
        /// <param name="maxPoolSize"></param>
        public void Configure(int startPoolSize, int maxPoolSize)
        {
            if (startPoolSize > maxPoolSize) throw new ArgumentException("Start Size must be less than max size", nameof(startPoolSize));

            if (this.maxPoolSize != maxPoolSize)
            {
                this.maxPoolSize = maxPoolSize;
                Array.Resize(ref pool, maxPoolSize);
            }

            for (int i = created; i < startPoolSize; i++)
            {
                Put(CreateNewBuffer());
            }

            if (logger.IsLogTypeAllowed(LogType.Log)) logger.Log(LogType.Log, $"Configuring buffer, start Size {startPoolSize}, max size {maxPoolSize}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferSize">size of each buffer</param>
        /// <param name="startPoolSize">how many buffers to create at start</param>
        /// <param name="maxPoolSize">max number of buffers in pool</param>
        /// <param name="logger"></param>
        public Pool(CreateNewItem createNew, int bufferSize, int startPoolSize, int maxPoolSize, ILogger logger = null)
        {
            if (startPoolSize > maxPoolSize) throw new ArgumentException("Start size must be less than max size", nameof(startPoolSize));
            this.createNew = createNew ?? throw new ArgumentNullException(nameof(createNew));

            this.bufferSize = bufferSize;
            this.maxPoolSize = maxPoolSize;
            this.logger = logger ?? Debug.unityLogger;

            pool = new T[maxPoolSize];
            for (int i = 0; i < startPoolSize; i++)
            {
                Put(CreateNewBuffer());
            }
        }

        private T CreateNewBuffer()
        {
            if (created >= maxPoolSize && logger.IsLogTypeAllowed(LogType.Warning)) logger.Log(LogType.Warning, $"Pool Max Size reached, type:{typeof(T).Name} created:{created + 1} max:{maxPoolSize}");
            created++;
            return createNew.Invoke(bufferSize, this);
        }

        public T Take()
        {
            if (next == PoolEmpty)
            {
                return CreateNewBuffer();
            }
            else
            {
                // todo is it a security risk to not clear buffer?

                // take then decrement
                T item = pool[next];
                pool[next] = null;
                next--;
                return item;
            }
        }

        public void Put(T buffer)
        {
            if (next < maxPoolSize - 1)
            {
                // increment then put
                pool[++next] = buffer;
            }
            else
            {
                if (logger.IsLogTypeAllowed(LogType.Warning)) logger.Log(LogType.Warning, $"Cant Put buffer into full pool, leaving for GC");
                // buffer is left for GC, so decrement created
                created--;
            }
        }
    }
}
