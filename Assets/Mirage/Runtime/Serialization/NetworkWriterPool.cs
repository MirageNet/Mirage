using System;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class NetworkWriterPool
    {
        static readonly ILogger logger;
        static readonly Pool<PooledNetworkWriter> pool;

        static NetworkWriterPool()
        {
            logger = LogFactory.GetLogger(typeof(NetworkReaderPool));
            // todo config
            pool = new Pool<PooledNetworkWriter>(PooledNetworkWriter.CreateNew, 1300, 5, 100, logger);
        }

        public static PooledNetworkWriter GetWriter()
        {
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
