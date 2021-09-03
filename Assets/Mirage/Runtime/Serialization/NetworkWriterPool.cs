using System;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class NetworkWriterPool
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkWriterPool));
        static Pool<PooledNetworkWriter> pool;

        /// <summary>
        /// Current Size of buffers, or null before Configure has been called
        /// </summary>
        public static int? BufferSize { get; private set; }

        static NetworkWriterPool()
        {
            // auto configure so that pool can be used without having to manually call it
            var config = new Config();
            Configure(config.MaxPacketSize);
        }

        /// <summary>
        /// Configures an exist pool or creates a new one
        /// <para>Does not create a new pool if <paramref name="bufferSize"/> is less that current <see cref="BufferSize"/></para>
        /// </summary>
        /// <param name="bufferSize">starting capacity of buffer</param>
        /// <param name="startPoolSize"></param>
        /// <param name="maxPoolSize"></param>
        public static void Configure(int bufferSize, int startPoolSize = 5, int maxPoolSize = 100)
        {
            // if new size is less, then just configure start/max
            if (BufferSize.HasValue && bufferSize < BufferSize.Value)
            {
                pool.Configure(startPoolSize, maxPoolSize);
            }
            else
            {
                pool = new Pool<PooledNetworkWriter>(PooledNetworkWriter.CreateNew, bufferSize, startPoolSize, maxPoolSize, logger);
                BufferSize = bufferSize;
            }
        }

        public static PooledNetworkWriter GetWriter()
        {
            if (pool == null) throw new InvalidOperationException("Configure must be called before ");
            PooledNetworkWriter writer = pool.Take();
            writer.Reset();
            return writer;
        }
    }

    /// <summary>
    /// NetworkWriter to be used with <see cref="NetworkWriterPool">NetworkWriterPool</see>
    /// </summary>
    public sealed class PooledNetworkWriter : NetworkWriter, IDisposable
    {
        private readonly Pool<PooledNetworkWriter> pool;

        private PooledNetworkWriter(int bufferSize, Pool<PooledNetworkWriter> pool) : base(bufferSize)
        {
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public static PooledNetworkWriter CreateNew(int bufferSize, Pool<PooledNetworkWriter> pool)
        {
            return new PooledNetworkWriter(bufferSize, pool);
        }

        /// <summary>
        /// Puts object back in Pool
        /// </summary>
        public void Release()
        {
            Reset();
            pool.Put(this);
        }

        void IDisposable.Dispose() => Release();
    }
}
