using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Warpper around a byte[] that belongs to a <see cref="Pool{T}"/>
    /// </summary>
    public sealed class ByteBuffer : IDisposable
    {
        public readonly byte[] array;
        readonly Pool<ByteBuffer> pool;

        private ByteBuffer(int bufferSize, Pool<ByteBuffer> pool)
        {
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));

            array = new byte[bufferSize];
        }

        public static ByteBuffer CreateNew(int bufferSize, Pool<ByteBuffer> pool)
        {
            return new ByteBuffer(bufferSize, pool);
        }

        public void Release()
        {
            pool.Put(this);
        }

        void IDisposable.Dispose() => Release();
    }
}
