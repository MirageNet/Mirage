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

        readonly int bufferSize;
        readonly int maxPoolSize;
        readonly ILogger logger;
        readonly Func<int, Pool<T>, T> createNew;


        readonly T[] pool;
        int next = -1;
        int created = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferSize">size of each buffer</param>
        /// <param name="startPoolSize">how many buffers to create at start</param>
        /// <param name="maxPoolSize">max number of buffers in pool</param>
        /// <param name="logger"></param>
        public Pool(Func<int, Pool<T>, T> createNew, int bufferSize, int startPoolSize, int maxPoolSize, ILogger logger = null)
        {
            if (startPoolSize > maxPoolSize) throw new ArgumentException("Start Size must be less than max size", nameof(startPoolSize));

            this.bufferSize = bufferSize;
            this.maxPoolSize = maxPoolSize;
            this.logger = logger ?? Debug.unityLogger;
            this.createNew = createNew;

            pool = new T[maxPoolSize];
            for (int i = 0; i < startPoolSize; i++)
            {
                Put(CreateNewBuffer());
            }
        }

        private T CreateNewBuffer()
        {
            if (created >= maxPoolSize && logger.IsLogTypeAllowed(LogType.Warning)) logger.Log(LogType.Warning, $"Buffer Max Size reached, created:{created} max:{maxPoolSize}");
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
                // todo is it a security risk to now clear buffer?

                // take then decriment
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
