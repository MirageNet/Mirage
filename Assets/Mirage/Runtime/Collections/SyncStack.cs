using System;
using System.Collections;
using System.Collections.Generic;
using Mirage.Serialization;

namespace Mirage.Collections
{
    public class SyncStack<T> : IReadOnlyCollection<T>, IEnumerable<T>, ISyncObject
    {
        private readonly Stack<T> _objects;

        public int Count => _objects.Count;
        public bool IsReadOnly { get; private set; }
        void ISyncObject.SetShouldSyncFrom(bool shouldSync) => IsReadOnly = !shouldSync;
        void ISyncObject.SetNetworkBehaviour(NetworkBehaviour networkBehaviour) { }

        /// <summary>
        /// Raised when an element is added to the list.
        /// Receives index and new item
        /// </summary>
        public event Action<T> OnPush;

        /// <summary>
        /// Raised when the list is cleared
        /// </summary>
        public event Action OnClear;

        /// <summary>
        /// Raised when an item is removed from the list
        /// receives the index and the old item
        /// </summary>
        public event Action<T> OnPop;

        /// <summary>
        /// Raised after the list has been updated
        /// Note that if there are multiple changes
        /// this event is only raised once.
        /// </summary>
        public event Action OnChange;

        private enum Operation : byte
        {
            OP_PUSH,
            OP_POP,
            OP_CLEAR,
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

        internal int ChangeCount => _changes.Count;

        public SyncStack()
        {
            _objects = new Stack<T>();
        }

        public SyncStack(Stack<T> objects)
        {
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

        private void AddOperation(Operation op, T newItem)
        {
            SyncObjectUtils.ThrowIfReadOnly(IsReadOnly);

            var change = new Change
            {
                Operation = op,
                Item = newItem
            };

            _changes.Add(change);
            OnChange?.Invoke();
        }

        public void OnSerializeAll(NetworkWriter writer)
        {
            // if init,  write the full list content
            writer.WritePackedUInt32((uint)_objects.Count);

            foreach (var item in _objects)
            {
                writer.Write(item);
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

                // only need too write item if pushing
                if (change.Operation == Operation.OP_PUSH)
                    writer.Write(change.Item);
            }
        }

        private static T[] _temp = Array.Empty<T>();
        public void OnDeserializeAll(NetworkReader reader)
        {
            // if init,  write the full list content
            var count = (int)reader.ReadPackedUInt32();

            _objects.Clear();
            OnClear?.Invoke();
            _changes.Clear();

            if (_temp.Length < count)
                Array.Resize(ref _temp, count);

            for (var i = 0; i < count; i++)
            {
                _temp[i] = reader.Read<T>();
            }

            for (var i = count - 1; i >= 0; i--)
            {
                _objects.Push(_temp[i]);
                OnPush?.Invoke(_temp[i]);
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
                    case Operation.OP_PUSH:
                        DeserializePush(reader, apply);
                        break;

                    case Operation.OP_CLEAR:
                        DeserializeClear(apply);
                        break;

                    case Operation.OP_POP:
                        DeserializePop(apply);
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

        private void DeserializePush(NetworkReader reader, bool apply)
        {
            var newItem = reader.Read<T>();
            if (apply)
            {
                _objects.Push(newItem);
                OnPush?.Invoke(newItem);
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

        private void DeserializePop(bool apply)
        {
            if (apply)
            {
                var oldItem = _objects.Pop();
                OnPop?.Invoke(oldItem);
            }
        }

        public void Push(T item)
        {
            _objects.Push(item);
            OnPush?.Invoke(item);
            AddOperation(Operation.OP_PUSH, item);
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var entry in range)
            {
                Push(entry);
            }
        }

        public void Clear()
        {
            _objects.Clear();
            OnClear?.Invoke();
            AddOperation(Operation.OP_CLEAR, default);
        }

        public void CopyTo(T[] array, int arrayIndex) => _objects.CopyTo(array, arrayIndex);

        public T Pop()
        {
            var item = _objects.Pop();
            OnPop?.Invoke(item);
            AddOperation(Operation.OP_POP, default);
            return item;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();
    }
}
