using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Threading;

using Supremacy.Annotations;

namespace Supremacy.Collections
{
    [Serializable]
    [ComVisible(false)]
    public sealed class ReadOnlyCollectionBase<T> : IObservableIndexedCollection<T>, IList<T>, IList
    {
        public static readonly ReadOnlyCollectionBase<T> Empty = new ReadOnlyCollectionBase<T>(new List<T>(0));

        private readonly IList<T> _list;
        [NonSerialized]
        private Object _syncRoot;

        public ReadOnlyCollectionBase([NotNull] IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            _list = list;
        }

        public int Count
        {
            get {
                lock (SyncRoot)
                {
                    return _list.Count;
                }
            }
        }

        public T this[int index]
        {
            get {
                lock (SyncRoot)
                {
                    return _list[index];
                }
            }
        }

        public bool Contains(T value)
        {
            lock (SyncRoot)
            {
                return _list.Contains(value);
            }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (SyncRoot)
            {
                _list.CopyTo(array, index);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _list.GetEnumerator();
            }
        }

        public int IndexOf(T value)
        {
            lock (SyncRoot)
            {
                return _list.IndexOf(value);
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        T IList<T>.this[int index]
        {
            get { return _list[index]; }
            set { throw NotSupportedOnReadOnlyCollection(); }
        }

        void ICollection<T>.Add(T value)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void ICollection<T>.Clear()
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void IList<T>.Insert(int index, T value)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        bool ICollection<T>.Remove(T value)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    var c = _list as ICollection;
                    if (c != null)
                    {
                        _syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref _syncRoot, new Object(), null);
                    }
                }
                return _syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.Rank != 1)
                throw new ArgumentException("Destination array must be single-dimensional.", "array");

            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("Destination array has non-zero lower bound.", "array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Index must be non-negative.");

            if ((array.Length - index) < Count)
                throw new ArgumentException("Destination array is not large enough.", "array");

            var items = array as T[];
            if (items != null)
            {
                lock (SyncRoot)
                {
                    _list.CopyTo(items, index);
                }
            }
            else
            {
                // 
                // Catch the obvious case assignment will fail.
                // We can found all possible problems by doing the check though.
                // For example, if the element type of the Array is derived from T,
                // we can't figure out if we can successfully copy the element beforehand. 
                //
                var targetType = array.GetType().GetElementType();
                var sourceType = typeof(T);
                if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                    throw new ArgumentException("Destination array is incompatible with this collection type.");

                //
                // We can't cast array of value type to object[], so we don't support 
                // widening of primitive types here.
                // 
                var objects = array as object[];
                if (objects == null)
                    throw new ArgumentException("Destination array is incompatible with this collection type.");

                lock (SyncRoot)
                {
                    var count = _list.Count;
                    try
                    {
                        for (var i = 0; i < count; i++)
                            objects[index++] = _list[i];
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentException("Destination array is incompatible with this collection type.");
                    }
                }
            }
        }

        bool IList.IsFixedSize
        {
            get { return true; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                throw NotSupportedOnReadOnlyCollection();
            }
        }

        int IList.Add(object value)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void IList.Clear()
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        private static bool IsCompatibleObject(object value)
        {
            if ((value is T) || (value == null && !typeof(T).IsValueType))
                return true;
            return false;
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
                return Contains((T)value);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
                return IndexOf((T)value);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void IList.Remove(object value)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void IList.RemoveAt(int index)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        private static InvalidOperationException NotSupportedOnReadOnlyCollection()
        {
            return new InvalidOperationException("Operation not supported on read-only collections.");
        }

        #region Implementation of INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                lock (SyncRoot)
                {
                    var source = _list as INotifyCollectionChanged; 
                    if (source != null)
                        source.CollectionChanged += value;
                }
            }
            remove
            {
                lock (SyncRoot)
                {
                    var source = _list as INotifyCollectionChanged;
                    if (source != null)
                        source.CollectionChanged -= value;
                }
            }
        }
        #endregion
    }
}