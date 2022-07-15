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
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkReaderPool));
        private static Pool<PooledNetworkReader> pool = new Pool<PooledNetworkReader>(PooledNetworkReader.CreateNew, 0, 5, 100, logger);

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

        /// <summary>
        /// Gets reader from pool. sets internal array and objectLocator values
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="objectLocator">Can be null, but must be set in order to read NetworkIdentity Values</param>
        /// <returns></returns>
        public static PooledNetworkReader GetReader(ArraySegment<byte> packet, IObjectLocator objectLocator)
        {
            var reader = pool.Take();
            reader.ObjectLocator = objectLocator;
            reader.Reset(packet.Array, packet.Offset, packet.Count);
            return reader;
        }
        /// <summary>
        /// Gets reader from pool. sets internal array and objectLocator values
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="objectLocator">Can be null, but must be set in order to read NetworkIdentity Values</param>
        /// <returns></returns>
        public static PooledNetworkReader GetReader(byte[] array, IObjectLocator objectLocator)
        {
            var reader = pool.Take();
            reader.ObjectLocator = objectLocator;
            reader.Reset(array, 0, array.Length);
            return reader;
        }
        /// <summary>
        /// Gets reader from pool. sets internal array and objectLocator values
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="objectLocator">Can be null, but must be set in order to read NetworkIdentity Values</param>
        /// <returns></returns>
        public static PooledNetworkReader GetReader(byte[] array, int offset, int length, IObjectLocator objectLocator)
        {
            var reader = pool.Take();
            reader.ObjectLocator = objectLocator;
            reader.Reset(array, offset, length);
            return reader;
        }
    }

    /// <summary>
    /// NetworkReader but has a ObjectLocator field that can be used by Reader functions to fetch NetworkIdentity
    /// </summary>
    public class MirageNetworkReader : NetworkReader
    {
        /// <summary>
        /// Used to find objects by net id
        /// </summary>
        public IObjectLocator ObjectLocator { get; set; }
    }

    /// <summary>
    /// NetworkReader to be used with <see cref="NetworkReaderPool">NetworkReaderPool</see>
    /// </summary>
    public sealed class PooledNetworkReader : MirageNetworkReader, IDisposable
    {
        private readonly Pool<PooledNetworkReader> _pool;

        private PooledNetworkReader(Pool<PooledNetworkReader> pool) : base()
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
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
                _pool.Put(this);
            }
        }
    }
}
