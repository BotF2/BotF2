// 
// EnumValueCollection.cs
// 
// Copyright (c) 2013-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Collections;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.IO.Serialization;

namespace Supremacy.Collections
{
    [Serializable]
    public sealed class EnumValueCollection<T> : IIndexedCollection<T>, IList<T>, IList, IOwnedDataSerializableAndRecreatable
        where T : struct
    {
        public static readonly EnumValueCollection<T> AllValues = new EnumValueCollection<T>();

        private T[] _values;

        public EnumValueCollection([NotNull] params T[] values)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("{0} is not an enum type.", typeof(T).FullName));

            if (values == null)
                throw new ArgumentNullException("values");

            _values = values;
        }

        public EnumValueCollection()
            : this((T[])Enum.GetValues(typeof(T))) {}

        public T[] Values
        {
            get { return _values; }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (var i = 0; i < _values.Length; i++)
                yield return _values[i];
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        public void Clear()
        {
            Array.Clear(_values, 0, _values.Length);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0, j = arrayIndex; i < _values.Length; i++, j++)
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
            return Array.IndexOf(_values, item, 0, _values.Length) - 0;
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
                return _values[index + 0];
            }
            set { throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection); }
        }

        private void VerifyIndexWithinBounds(int index)
        {
            if (index < 0 || index >= _values.Length)
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
            Array.Copy(_values, 0, array, index, _values.Length);
        }

        int ICollection.Count
        {
            get { return _values.Length; }
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
            return Array.IndexOf(_values, value, 0, _values.Length);
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
                return _values[index];
            }
            set { throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection); }
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

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _values = reader.ReadArray<T>();
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteArray(_values);
        }

        #endregion

        #region Enumerator

        [Serializable]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly EnumValueCollection<T> _owner;
            private int _index;
            private T _current;

            internal Enumerator(EnumValueCollection<T> owner)
            {
                _owner = owner;
                _index = 0;
                _current = default(T);
            }

            public void Dispose() {}

            public bool MoveNext()
            {
                var owner = _owner;

                if (_index < owner.Count)
                {
                    _current = owner._values[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                _index = _owner.Count + 1;
                _current = default(T);
                return false;
            }

            public T Current
            {
                get { return _current; }
            }

            Object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _owner.Count + 1)
                        throw new InvalidOperationException();
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default(T);
            }
        }

        #endregion
    }
}
