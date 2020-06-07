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
        private object _syncRoot;

        public ReadOnlyCollectionBase([NotNull] IList<T> list)
        {
            _list = list ?? throw new ArgumentNullException(nameof(list));
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _list.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
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

        public int IndexOf(T item)
        {
            lock (SyncRoot)
            {
                return _list.IndexOf(item);
            }
        }

        bool ICollection<T>.IsReadOnly => true;

        T IList<T>.this[int index]
        {
            get => _list[index];
            set => throw NotSupportedOnReadOnlyCollection();
        }

        void ICollection<T>.Add(T value)
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void ICollection<T>.Clear()
        {
            throw NotSupportedOnReadOnlyCollection();
        }

        void IList<T>.Insert(int index, T item)
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

        public bool IsSynchronized => true;

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    if (_list is ICollection c)
                    {
                        _syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        _ = Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                    }
                }
                return _syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException("Destination array must be single-dimensional.", nameof(array));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("Destination array has non-zero lower bound.", nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
            }

            if ((array.Length - index) < Count)
            {
                throw new ArgumentException("Destination array is not large enough.", nameof(array));
            }

            if (array is T[] items)
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
                Type targetType = array.GetType().GetElementType();
                Type sourceType = typeof(T);
                if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                {
                    throw new ArgumentException("Destination array is incompatible with this collection type.");
                }

                //
                // We can't cast array of value type to object[], so we don't support 
                // widening of primitive types here.
                // 
                if (!(array is object[] objects))
                {
                    throw new ArgumentException("Destination array is incompatible with this collection type.");
                }

                lock (SyncRoot)
                {
                    int count = _list.Count;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            objects[index++] = _list[i];
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentException("Destination array is incompatible with this collection type.");
                    }
                }
            }
        }

        bool IList.IsFixedSize => true;

        bool IList.IsReadOnly => true;

        object IList.this[int index]
        {
            get => this[index];
            set => throw NotSupportedOnReadOnlyCollection();
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
            return (value is T) || (value == null && !typeof(T).IsValueType);
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T)value);
            }

            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T)value);
            }

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
                    if (_list is INotifyCollectionChanged source)
                    {
                        source.CollectionChanged += value;
                    }
                }
            }
            remove
            {
                lock (SyncRoot)
                {
                    if (_list is INotifyCollectionChanged source)
                    {
                        source.CollectionChanged -= value;
                    }
                }
            }
        }
        #endregion
    }
}