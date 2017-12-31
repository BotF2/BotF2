using System;
using System.Collections;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.IO.Serialization;

namespace Supremacy.Collections
{
    [Serializable]
    public class ArrayWrapper<T> : IIndexedCollection<T>, IList<T>, IList, IOwnedDataSerializableAndRecreatable
    {
        // ReSharper disable StaticFieldInGenericType
        public static readonly ArrayWrapper<T> Empty = new ArrayWrapper<T>(new T[0]);
        // ReSharper restore StaticFieldInGenericType

        private T[] _values;
        private readonly int _start;
        private readonly int _count;

        public ArrayWrapper([NotNull] T[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            _values = values;
            _count = values.Length;
        }

        public ArrayWrapper([NotNull] T[] values, int start, int count)
            : this(values)
        {
            if (start < 0 || start >= values.Length)
            {
                throw new ArgumentOutOfRangeException(
                    "start",
                    SR.ArgumentOutOfRangeException_IndexIsOutsideArrayBounds);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "count",
                    SR.ArgumentOutOfRangeException_ValueMustBeNonNegative);
            }
            
            if (start + count > values.Length)
            {
                throw new ArgumentOutOfRangeException(
                    "count",
                    SR.ArgumentOutOfRangeException_StartPlusCountExceedsLength);
            }

            _start = start;
            _count = count;
        }

        public ArrayWrapper()
        {
            // For serialization purposes only.
            _values = Empty._values;
        }

        public ArrayWrapper(int length)
        {
            // For serialization purposes only.
            _values = new T[length];
        }

        public T[] Values
        {
            get { return _values; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _start + _count; i++)
                yield return _values[i];
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        public void Clear()
        {
            Array.Clear(_values, _start, _count);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0, j = arrayIndex; i < _start + _count; i++, j++)
                array[i] = _values[i];
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        public int Count
        {
            get { return _values.Length; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_values, item, _start, _count) - _start;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        public T this[int index]
        {
            get
            {
                VerifyIndexWithinBounds(index);
                return _values[index + _start];
            }
            set
            {
                VerifyIndexWithinBounds(index); 
                _values[index + _start] = value;
            }
        }

        private void VerifyIndexWithinBounds(int index)
        {
            if (index < _start ||
                index >= (_start + _count))
            {
                throw new ArgumentOutOfRangeException(
                    "index",
                    SR.ArgumentOutOfRangeException_IndexIsOutsideArrayBounds);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Array.Copy(_values, _start, array, index, _count);
        }

        int ICollection.Count
        {
            get { return _count; }
        }

        object ICollection.SyncRoot
        {
            get { return _values.SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        bool IList.Contains(object value)
        {
            return ((IList)this).IndexOf(value) >= 0;
        }

        void IList.Clear()
        {
            Clear();
        }

        int IList.IndexOf(object value)
        {
            return Array.IndexOf(_values, value, _start, _count);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        object IList.this[int index]
        {
            get
            {
                VerifyIndexWithinBounds(index);
                return _values[index + _start];
            }
            set
            {
                VerifyIndexWithinBounds(index);
                ((IList)_values)[index + _start] = value;
            }
        }

        bool IList.IsReadOnly
        {
            get { return _values.IsReadOnly; }
        }

        bool IList.IsFixedSize
        {
            get { return true; }
        }

        #region Implementation of IOwnedDataSerializable
        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _values = reader.ReadArray<T>() ??
                      Empty._values;
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteArray(_values);
        }
        #endregion
    }
}