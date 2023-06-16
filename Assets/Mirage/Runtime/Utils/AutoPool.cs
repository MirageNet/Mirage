using System;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Pool class that will create a Disposable wrapper around T so it can be used with any class automatically without additional setup
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class AutoPool<T> where T : class, new()
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(AutoPool<T>));

        /// <summary>
        /// Default pool instance, safe to use on main thread
        /// </summary>
        public static Pool<Wrapper> Pool;

        static AutoPool()
        {
            Pool = new Pool<Wrapper>(p => new Wrapper(p), 1, 10, logger);
        }

        public static Wrapper Take() => Pool.Take();

        public class Wrapper : IDisposable
        {
            private readonly Pool<Wrapper> _pool;
            public readonly T Item;

            public Wrapper(Pool<Wrapper> pool)
            {
                _pool = pool;
                Item = new T();
            }

            public void Dispose() => _pool.Put(this);

            public static implicit operator T(Wrapper wrapper) => wrapper.Item;
        }
    }
}
