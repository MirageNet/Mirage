using System;
using System.Collections;
using System.Collections.Generic;
using Mirage.Serialization;

namespace Mirage.Collections
{
    public class SyncSet<T> : ISet<T>, ISyncObject
    {
        protected readonly ISet<T> objects;

        public int Count => objects.Count;
        public bool IsReadOnly { get; private set; }
        void ISyncObject.SetShouldSyncFrom(bool shouldSync) => IsReadOnly = !shouldSync;
        void ISyncObject.SetNetworkBehaviour(NetworkBehaviour networkBehaviour) { }

        internal int ChangeCount => _changes.Count;

        /// <summary>
        /// Raised when an element is added to the list.
        /// Receives the new item
        /// </summary>
        public event Action<T> OnAdd;

        /// <summary>
        /// Raised when the set is cleared
        /// </summary>
        public event Action OnClear;

        /// <summary>
        /// Raised when an item is removed from the set
        /// receives the old item
        /// </summary>
        public event Action<T> OnRemove;

        /// <summary>
        /// Raised after the set has been updated
        /// Note that if there are multiple changes
        /// this event is only raised once.
        /// </summary>
        public event Action OnChange;

        private enum Operation : byte
        {
            OP_ADD,
            OP_CLEAR,
            OP_REMOVE,
            OP_FULL_SYNC
        }

        private struct Change
        {
            public Operation Operation;
            public T Item;
        }

        private readonly List<Change> _changes = new List<Change>();

        // how many changes we need to ignore
        // this is needed because when we initialize the list,
        // we might later receive changes that have already been applied
        // so we need to skip them
        private int _changesAhead;

        public readonly int MaxElements;

        public SyncSet(ISet<T> objects, int maxElements)
        {
            this.objects = objects;
            MaxElements = maxElements;
        }

        public void Reset()
        {
            IsReadOnly = false;
            _changes.Clear();
            _changesAhead = 0;
            objects.Clear();
        }

        public bool IsDirty => _changes.Count > 0;

        // throw away all the changes
        // this should be called after a successfull sync
        public void Flush() => _changes.Clear();

        private void AddOperation(Operation op) => AddOperation(op, default);

        private void AddOperation(Operation op, T item)
        {
            SyncObjectUtils.ThrowIfReadOnly(IsReadOnly);

            var change = new Change
            {
                Operation = op,
                Item = item
            };

            _changes.Add(change);
            OnChange?.Invoke();
        }

        public void OnSerializeAll(NetworkWriter writer)
        {
            // if init,  write the full list content
            writer.WritePackedUInt32((uint)objects.Count);

            foreach (var obj in objects)
            {
                writer.Write(obj);
            }

            // all changes have been applied already
            // thus the client will need to skip all the pending changes
            // or they would be applied again.
            // So we write how many changes are pending
            writer.WritePackedUInt32((uint)_changes.Count);
        }

        public void OnSerializeDelta(NetworkWriter writer)
        {
            // Syncing a massive count of individual changes is insecure and highly inefficient.
            // We optimize and secure egress by sending a single full synchronization instead.
            if (_changes.Count > 100 && _changes.Count > objects.Count)
            {
                writer.WritePackedUInt32(1);
                writer.WriteByte((byte)Operation.OP_FULL_SYNC);
                writer.WritePackedUInt32((uint)objects.Count);
                foreach (var syncItem in objects)
                    writer.Write(syncItem);
                return;
            }

            // write all the queued up changes
            writer.WritePackedUInt32((uint)_changes.Count);

            for (var i = 0; i < _changes.Count; i++)
            {
                var change = _changes[i];
                writer.WriteByte((byte)change.Operation);

                switch (change.Operation)
                {
                    case Operation.OP_ADD:
                        writer.Write(change.Item);
                        break;

                    case Operation.OP_CLEAR:
                        break;

                    case Operation.OP_REMOVE:
                        writer.Write(change.Item);
                        break;
                }
            }
        }

        public void OnDeserializeAll(NetworkReader reader)
        {
            // if init,  write the full list content
            var count = (int)reader.ReadPackedUInt32();

            if (count > MaxElements)
                throw new InvalidOperationException($"SyncSet capacity would exceed MaxElements limit of {MaxElements}");

            objects.Clear();
            _changes.Clear();
            OnClear?.Invoke();

            for (var i = 0; i < count; i++)
            {
                var obj = reader.Read<T>();
                objects.Add(obj);
                OnAdd?.Invoke(obj);
            }

            // We will need to skip all these changes
            // the next time the list is synchronized
            // because they have already been applied
            _changesAhead = (int)reader.ReadPackedUInt32();
            OnChange?.Invoke();
        }

        public void OnDeserializeDelta(NetworkReader reader)
        {
            var raiseOnChange = false;

            var changesCount = (int)reader.ReadPackedUInt32();

            for (var i = 0; i < changesCount; i++)
            {
                var operation = (Operation)reader.ReadByte();

                // apply the operation only if it is a new change
                // that we have not applied yet
                var apply = _changesAhead == 0;

                switch (operation)
                {
                    case Operation.OP_ADD:
                        DeserializeAdd(reader, apply);
                        break;

                    case Operation.OP_CLEAR:
                        DeserializeClear(apply);
                        break;

                    case Operation.OP_REMOVE:
                        DeserializeRemove(reader, apply);
                        break;

                    case Operation.OP_FULL_SYNC:
                        DeserializeFullSync(reader, apply);
                        break;
                }

                if (apply)
                {
                    raiseOnChange = true;
                }
                // we just skipped this change
                else
                {
                    if (_changesAhead > 0)
                        _changesAhead--;
                }
            }

            if (raiseOnChange)
            {
                OnChange?.Invoke();
            }
        }

        private void DeserializeFullSync(NetworkReader reader, bool apply)
        {
            var count = (int)reader.ReadPackedUInt32();
            if (MaxElements.HasValue && count > MaxElements.Value)
                throw new InvalidOperationException($"SyncSet capacity would exceed MaxElements limit of {MaxElements.Value}");

            if (apply)
            {
                objects.Clear();
                OnClear?.Invoke();
                for (var i = 0; i < count; i++)
                {
                    var item = reader.Read<T>();
                    objects.Add(item);
                    OnAdd?.Invoke(item);
                }
            }
            else
                for (var i = 0; i < count; i++)
                    reader.Read<T>();

            _changesAhead = 0;
        }

        private void DeserializeAdd(NetworkReader reader, bool apply)
        {
            var item = reader.Read<T>();
            if (apply)
            {
                if (!objects.Contains(item) && objects.Count >= MaxElements)
                    throw new InvalidOperationException($"SyncSet capacity would exceed MaxElements limit of {MaxElements}");

                objects.Add(item);
                OnAdd?.Invoke(item);
            }
        }

        private void DeserializeClear(bool apply)
        {
            if (apply)
            {
                objects.Clear();
                OnClear?.Invoke();
            }
        }

        private void DeserializeRemove(NetworkReader reader, bool apply)
        {
            var item = reader.Read<T>();
            if (apply)
            {
                objects.Remove(item);
                OnRemove?.Invoke(item);
            }
        }

        public bool Add(T item)
        {
            if (!objects.Contains(item) && objects.Count >= MaxElements)
                throw new InvalidOperationException($"SyncSet capacity would exceed MaxElements limit of {MaxElements}");

            if (objects.Add(item))
            {
                OnAdd?.Invoke(item);
                AddOperation(Operation.OP_ADD, item);
                return true;
            }
            return false;
        }

        void ICollection<T>.Add(T item) => _ = Add(item);

        public void Clear()
        {
            objects.Clear();
            OnClear?.Invoke();
            AddOperation(Operation.OP_CLEAR);
        }

        public bool Contains(T item) => objects.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => objects.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            if (objects.Remove(item))
            {
                OnRemove?.Invoke(item);
                AddOperation(Operation.OP_REMOVE, item);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator() => objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == this)
            {
                Clear();
                return;
            }

            // remove every element in other from this
            foreach (var element in other)
            {
                Remove(element);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other is ISet<T> otherSet)
            {
                IntersectWithSet(otherSet);
            }
            else
            {
                var otherAsSet = new HashSet<T>(other);
                IntersectWithSet(otherAsSet);
            }
        }

        private void IntersectWithSet(ISet<T> otherSet)
        {
            var elements = new List<T>(objects);

            foreach (var element in elements)
            {
                if (!otherSet.Contains(element))
                {
                    Remove(element);
                }
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) => objects.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => objects.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => objects.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => objects.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => objects.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => objects.SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == this)
            {
                Clear();
            }
            else
            {
                foreach (var element in other)
                {
                    if (!Remove(element))
                    {
                        Add(element);
                    }
                }
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other != this)
            {
                foreach (var element in other)
                {
                    Add(element);
                }
            }
        }
    }

    public class SyncHashSet<T> : SyncSet<T>
    {
        public SyncHashSet(int maxElements, IEqualityComparer<T> comparer = null) : base(new HashSet<T>(comparer ?? EqualityComparer<T>.Default), maxElements) { }

        // allocation free enumerator
        public new HashSet<T>.Enumerator GetEnumerator() => ((HashSet<T>)objects).GetEnumerator();
    }

    public class SyncSortedSet<T> : SyncSet<T>
    {
        public SyncSortedSet(int maxElements, IComparer<T> comparer = null) : base(new SortedSet<T>(comparer ?? Comparer<T>.Default), maxElements) { }

        // allocation free enumerator
        public new SortedSet<T>.Enumerator GetEnumerator() => ((SortedSet<T>)objects).GetEnumerator();
    }
}
