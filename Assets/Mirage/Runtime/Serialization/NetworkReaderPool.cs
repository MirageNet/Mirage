using System;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Serialization
{
    /// <summary>
    /// Holds static reference to <see cref="Pool{T}"/> of <see cref="PooledNetworkReader"/>
    /// </summary>
    public static class NetworkReaderPool
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkReaderPool));
        static Pool<PooledNetworkReader> pool = new Pool<PooledNetworkReader>(PooledNetworkReader.CreateNew, 0, 5, 100, logger);

        public static void Configure(int startPoolSize = 5, int maxPoolSize = 100)
        {
            if (pool != null)
            {
                pool.Configure(startPoolSize, maxPoolSize);
            }
            else
            {
                pool = new Pool<PooledNetworkReader>(PooledNetworkReader.CreateNew, 0, startPoolSize, maxPoolSize, logger);
            }
        }

        public static PooledNetworkReader GetReader(ArraySegment<byte> packet)
        {
            PooledNetworkReader reader = pool.Take();
            reader.Reset(packet.Array, packet.Offset, packet.Count);
            return reader;
        }
        public static PooledNetworkReader GetReader(byte[] array)
        {
            PooledNetworkReader reader = pool.Take();
            reader.Reset(array, 0, array.Length);
            return reader;
        }
        public static PooledNetworkReader GetReader(byte[] array, int offset, int length)
        {
            PooledNetworkReader reader = pool.Take();
            reader.Reset(array, offset, length);
            return reader;
        }
    }

    /// <summary>
    /// NetworkReader to be used with <see cref="NetworkReaderPool">NetworkReaderPool</see>
    /// </summary>
    public sealed class PooledNetworkReader : NetworkReader, IDisposable
    {
        private readonly Pool<PooledNetworkReader> pool;

        private PooledNetworkReader(Pool<PooledNetworkReader> pool) : base()
        {
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public static PooledNetworkReader CreateNew(int _, Pool<PooledNetworkReader> pool)
        {
            return new PooledNetworkReader(pool);
        }

        /// <summary>
        /// Puts object back in Pool
        /// </summary>
        public void Release()
        {
            Dispose(true);
        }

        void IDisposable.Dispose() => Dispose(true);
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // only put back into the pool is Dispose was called
            // => dont put it back for finalize
            if (disposing)
            {
                pool.Put(this);
            }
        }
    }
}
