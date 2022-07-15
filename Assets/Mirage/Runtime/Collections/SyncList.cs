using System;
using System.Collections;
using System.Collections.Generic;
using Mirage.Serialization;

namespace Mirage.Collections
{
    public class SyncList<T> : IList<T>, IReadOnlyList<T>, ISyncObject
    {
        private readonly IList<T> _objects;
        private readonly IEqualityComparer<T> _comparer;

        public int Count => _objects.Count;
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Raised when an element is added to the list.
        /// Receives index and new item
        /// </summary>
        public event Action<int, T> OnInsert;

        /// <summary>
        /// Raised when the list is cleared
        /// </summary>
        public event Action OnClear;

        /// <summary>
        /// Raised when an item is removed from the list
        /// receives the index and the old item
        /// </summary>
        public event Action<int, T> OnRemove;

        /// <summary>
        /// Raised when an item is changed in a list
        /// Receives index, old item and new item
        /// </summary>
        public event Action<int, T, T> OnSet;

        /// <summary>
        /// Raised after the list has been updated
        /// Note that if there are multiple changes
        /// this event is only raised once.
        /// </summary>
        public event Action OnChange;

        private enum Operation : byte
        {
            OP_ADD,
            OP_CLEAR,
            OP_INSERT,
            OP_REMOVEAT,
            OP_SET
        }

        private struct Change
        {
            public Operation Operation;
            public int Index;
            public T Item;
        }

        private readonly List<Change> _changes = new List<Change>();

        // how many changes we need to ignore
        // this is needed because when we initialize the list,
        // we might later receive changes that have already been applied
        // so we need to skip them
        private int _changesAhead;

        internal int ChangeCount => _changes.Count;

        public SyncList() : this(EqualityComparer<T>.Default)
        {
        }

        public SyncList(IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _objects = new List<T>();
        }

        public SyncList(IList<T> objects, IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _objects = objects;
        }

        public bool IsDirty => _changes.Count > 0;

        // throw away all the changes
        // this should be called after a successfull sync
        public void Flush() => _changes.Clear();

        public void Reset()
        {
            IsReadOnly = false;
            _changes.Clear();
            _changesAhead = 0;
            _objects.Clear();
        }

        private void AddOperation(Operation op, int itemIndex, T newItem)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Synclists can only be modified at the server");
            }

            var change = new Change
            {
                Operation = op,
                Index = itemIndex,
                Item = newItem
            };

            _changes.Add(change);
            OnChange?.Invoke();
        }

        public void OnSerializeAll(NetworkWriter writer)
        {
            // if init,  write the full list content
            writer.WritePackedUInt32((uint)_objects.Count);

            for (var i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
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

                    case Operation.OP_REMOVEAT:
                        writer.WritePackedUInt32((uint)change.Index);
                        break;

                    case Operation.OP_INSERT:
                    case Operation.OP_SET:
                        writer.WritePackedUInt32((uint)change.Index);
                        writer.Write(change.Item);
                        break;
                }
            }
        }

        public void OnDeserializeAll(NetworkReader reader)
        {
            // This list can now only be modified by synchronization
            IsReadOnly = true;

            // if init,  write the full list content
            var count = (int)reader.ReadPackedUInt32();

            _objects.Clear();
            OnClear?.Invoke();
            _changes.Clear();

            for (var i = 0; i < count; i++)
            {
                var obj = reader.Read<T>();
                _objects.Add(obj);
                OnInsert?.Invoke(i, obj);
            }

            // We will need to skip all these changes
            // the next time the list is synchronized
            // because they have already been applied
            _changesAhead = (int)reader.ReadPackedUInt32();

            OnChange?.Invoke();
        }

        public void OnDeserializeDelta(NetworkReader reader)
        {
            // This list can now only be modified by synchronization
            IsReadOnly = true;
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

                    case Operation.OP_INSERT:
                        DeserializeInsert(reader, apply);
                        break;

                    case Operation.OP_REMOVEAT:
                        DeserializeRemoveAt(reader, apply);
                        break;

                    case Operation.OP_SET:
                        DeserializeSet(reader, apply);
                        break;
                }

                if (apply)
                {
                    raiseOnChange = true;
                }
                // we just skipped this change
                else
                {
                    _changesAhead--;
                }
            }

            if (raiseOnChange)
                OnChange?.Invoke();
        }

        private void DeserializeAdd(NetworkReader reader, bool apply)
        {
            var newItem = reader.Read<T>();
            if (apply)
            {
                _objects.Add(newItem);
                OnInsert?.Invoke(_objects.Count - 1, newItem);
            }
        }

        private void DeserializeClear(bool apply)
        {
            if (apply)
            {
                _objects.Clear();
                OnClear?.Invoke();
            }
        }

        private void DeserializeInsert(NetworkReader reader, bool apply)
        {
            var index = (int)reader.ReadPackedUInt32();
            var newItem = reader.Read<T>();
            if (apply)
            {
                _objects.Insert(index, newItem);
                OnInsert?.Invoke(index, newItem);
            }
        }

        private void DeserializeRemoveAt(NetworkReader reader, bool apply)
        {
            var index = (int)reader.ReadPackedUInt32();
            if (apply)
            {
                var oldItem = _objects[index];
                _objects.RemoveAt(index);
                OnRemove?.Invoke(index, oldItem);
            }
        }

        private void DeserializeSet(NetworkReader reader, bool apply)
        {
            var index = (int)reader.ReadPackedUInt32();
            var newItem = reader.Read<T>();
            if (apply)
            {
                var oldItem = _objects[index];
                _objects[index] = newItem;
                OnSet?.Invoke(index, oldItem, newItem);
            }
        }

        public void Add(T item)
        {
            _objects.Add(item);
            OnInsert?.Invoke(_objects.Count - 1, item);
            AddOperation(Operation.OP_ADD, _objects.Count - 1, item);
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var entry in range)
            {
                Add(entry);
            }
        }

        public void Clear()
        {
            _objects.Clear();
            OnClear?.Invoke();
            AddOperation(Operation.OP_CLEAR, 0, default);
        }

        public bool Contains(T item) => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex) => _objects.CopyTo(array, arrayIndex);

        public int IndexOf(T item)
        {
            for (var i = 0; i < _objects.Count; ++i)
            {
                if (_comparer.Equals(item, _objects[i]))
                    return i;
            }

            return -1;
        }

        public int FindIndex(Predicate<T> match)
        {
            for (var i = 0; i < _objects.Count; ++i)
            {
                if (match(_objects[i]))
                    return i;
            }

            return -1;
        }

        public T Find(Predicate<T> match)
        {
            var i = FindIndex(match);
            return (i != -1) ? _objects[i] : default;
        }

        public List<T> FindAll(Predicate<T> match)
        {
            var results = new List<T>();
            for (var i = 0; i < _objects.Count; ++i)
            {
                if (match(_objects[i]))
                    results.Add(_objects[i]);
            }

            return results;
        }

        public void Insert(int index, T item)
        {
            _objects.Insert(index, item);
            OnInsert?.Invoke(index, item);
            AddOperation(Operation.OP_INSERT, index, item);
        }

        public void InsertRange(int index, IEnumerable<T> range)
        {
            foreach (var entry in range)
            {
                Insert(index, entry);
                index++;
            }
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            var result = index >= 0;
            if (result)
            {
                RemoveAt(index);
            }
            return result;
        }

        public void RemoveAt(int index)
        {
            var oldItem = _objects[index];
            _objects.RemoveAt(index);
            OnRemove?.Invoke(index, oldItem);
            AddOperation(Operation.OP_REMOVEAT, index, default);
        }

        public int RemoveAll(Predicate<T> match)
        {
            var toRemove = new List<T>();
            for (var i = 0; i < _objects.Count; ++i)
            {
                if (match(_objects[i]))
                    toRemove.Add(_objects[i]);
            }

            foreach (var entry in toRemove)
            {
                Remove(entry);
            }

            return toRemove.Count;
        }

        public T this[int i]
        {
            get => _objects[i];
            set
            {
                if (!_comparer.Equals(_objects[i], value))
                {
                    var oldItem = _objects[i];
                    _objects[i] = value;
                    OnSet?.Invoke(i, oldItem, value);
                    AddOperation(Operation.OP_SET, i, value);
                }
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        // default Enumerator allocates. we need a custom struct Enumerator to
        // not allocate on the heap.
        // (System.Collections.Generic.List<T> source code does the same)
        //
        // benchmark:
        //   uMMORPG with 800 monsters, Skills.GetHealthBonus() which runs a
        //   foreach on skills SyncList:
        //      before: 81.2KB GC per frame
        //      after:     0KB GC per frame
        // => this is extremely important for MMO scale networking
        public struct Enumerator : IEnumerator<T>
        {
            private readonly SyncList<T> _list;
            private int _index;
            public T Current { get; private set; }

            public Enumerator(SyncList<T> list)
            {
                _list = list;
                _index = -1;
                Current = default;
            }

            public bool MoveNext()
            {
                if (++_index >= _list.Count)
                {
                    return false;
                }
                Current = _list[_index];
                return true;
            }

            public void Reset() => _index = -1;
            object IEnumerator.Current => Current;
            public void Dispose()
            {
                // nothing to dispose
            }
        }
    }
}
