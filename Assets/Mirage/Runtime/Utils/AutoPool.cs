using System;
using System.Collections;
using System.Collections.Generic;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Helpers for taking collections from <see cref="AutoPool{T}"/> so that they are cleared when returned to the pool
    /// <para>List and Dictionary are cleared by Wrapper.Dispose via IList/IDictionary, other collections need <see cref="AutoPool{T}.ClearAction"/> set by these helpers</para>
    /// </summary>
    public static class AutoPool
    {
        public static AutoPool<List<T>>.Wrapper TakeList<T>(out List<T> item)
        {
            return AutoPool<List<T>>.Take(out item);
        }

        public static AutoPool<Dictionary<TKey, TValue>>.Wrapper TakeDictionary<TKey, TValue>(out Dictionary<TKey, TValue> item)
        {
            return AutoPool<Dictionary<TKey, TValue>>.Take(out item);
        }

        // ClearAction is set here, where the element type is known, so Clear is compiled code instead of reflection (IL2CPP safe)
        public static AutoPool<HashSet<T>>.Wrapper TakeHashSet<T>(out HashSet<T> item)
        {
            AutoPool<HashSet<T>>.ClearAction ??= set => set.Clear();
            return AutoPool<HashSet<T>>.Take(out item);
        }

        public static AutoPool<Queue<T>>.Wrapper TakeQueue<T>(out Queue<T> item)
        {
            AutoPool<Queue<T>>.ClearAction ??= queue => queue.Clear();
            return AutoPool<Queue<T>>.Take(out item);
        }

        public static AutoPool<Stack<T>>.Wrapper TakeStack<T>(out Stack<T> item)
        {
            AutoPool<Stack<T>>.ClearAction ??= stack => stack.Clear();
            return AutoPool<Stack<T>>.Take(out item);
        }
    }

    /// <summary>
    /// Pool class that will create a Disposable wrapper around T so it can be used with any class automatically without additional setup
    /// </summary>
    /// <typeparam name="T">Type of item to pool. IList and IDictionary are cleared on return, other collections should be taken via <see cref="AutoPool"/> helpers</typeparam>
    public static class AutoPool<T> where T : class, new()
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(AutoPool<T>));

        /// <summary>
        /// Default pool instance, safe to use on main thread
        /// </summary>
        public static Pool<Wrapper> Pool;

        /// <summary>
        /// Called when an item is returned to the pool, for types that are not <see cref="IList"/> or <see cref="IDictionary"/>.
        /// <para>Shared by all users of this T. Set by the collection helpers in <see cref="AutoPool"/> on their first call,
        /// so items taken directly via <see cref="Take()"/> before that are not cleared</para>
        /// </summary>
        public static Action<T> ClearAction;

        static AutoPool()
        {
            Pool = new Pool<Wrapper>(p => new Wrapper(p), 1, 10, logger);
        }

        public static Wrapper Take() => Pool.Take();

        public static Wrapper Take(out T item)
        {
            var wrapper = Pool.Take();
            item = wrapper.Item;
            return wrapper;
        }

        public class Wrapper : IDisposable
        {
            private readonly Pool<Wrapper> _pool;
            public readonly T Item;

            public Wrapper(Pool<Wrapper> pool)
            {
                _pool = pool;
                Item = new T();
            }

            public void Dispose()
            {
                // clear collections so the next consumer doesn't receive stale items
                if (Item is IList list)
                    list.Clear();
                else if (Item is IDictionary dictionary)
                    dictionary.Clear();
                else
                    ClearAction?.Invoke(Item);
                _pool.Put(this);
            }

            public static implicit operator T(Wrapper wrapper) => wrapper.Item;
        }
    }
}
