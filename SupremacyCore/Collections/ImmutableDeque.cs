using System;
using System.Collections;
using System.Collections.Generic;

namespace Supremacy.Collections
{
    public interface IDeque<T> : IEnumerable<T>
    {
        #region Properties and Indexers
        bool IsEmpty { get; }
        #endregion

        #region Methods
        IDeque<T> DequeueLeft();
        IDeque<T> DequeueRight();
        IDeque<T> EnqueueLeft(T value);
        IDeque<T> EnqueueRight(T value);
        T PeekLeft();
        T PeekRight();
        #endregion
    }

    public sealed class ImmutableDeque<T> : IDeque<T>
    {
        #region Fields
        private static readonly IDeque<T> s_empty = new EmptyDeque();

        private readonly Dequelette _left;
        private readonly IDeque<Dequelette> _middle;
        private readonly Dequelette _right;
        #endregion

        #region Constructors
        private ImmutableDeque(Dequelette left, IDeque<Dequelette> middle, Dequelette right)
        {
            _left = left;
            _middle = middle;
            _right = right;
        }
        #endregion

        #region Properties and Indexers
        public static IDeque<T> Empty
        {
            get { return s_empty; }
        }
        #endregion

        #region IDeque<T> Members
        public bool IsEmpty
        {
            get { return false; }
        }

        public IDeque<T> EnqueueLeft(T value)
        {
            if (!_left.Full)
                return new ImmutableDeque<T>(_left.EnqueueLeft(value), _middle, _right);
            return new ImmutableDeque<T>(
                new Two(value, _left.PeekLeft()),
                _middle.EnqueueLeft(_left.DequeueLeft()),
                _right);
        }

        public IDeque<T> EnqueueRight(T value)
        {
            if (!_right.Full)
                return new ImmutableDeque<T>(_left, _middle, _right.EnqueueRight(value));
            return new ImmutableDeque<T>(
                _left,
                _middle.EnqueueRight(_right.DequeueRight()),
                new Two(_right.PeekRight(), value));
        }

        public IDeque<T> DequeueLeft()
        {
            if (_left.Size > 1)
                return new ImmutableDeque<T>(_left.DequeueLeft(), _middle, _right);
            if (!_middle.IsEmpty)
                return new ImmutableDeque<T>(_middle.PeekLeft(), _middle.DequeueLeft(), _right);
            if (_right.Size > 1)
                return new ImmutableDeque<T>(new One(_right.PeekLeft()), _middle, _right.DequeueLeft());
            return new SingleDeque(_right.PeekLeft());
        }

        public IDeque<T> DequeueRight()
        {
            if (_right.Size > 1)
                return new ImmutableDeque<T>(_left, _middle, _right.DequeueRight());
            if (!_middle.IsEmpty)
                return new ImmutableDeque<T>(_left, _middle.DequeueRight(), _middle.PeekRight());
            if (_left.Size > 1)
                return new ImmutableDeque<T>(_left.DequeueRight(), _middle, new One(_left.PeekRight()));
            return new SingleDeque(_left.PeekRight());
        }

        public T PeekLeft()
        {
            return _left.PeekLeft();
        }

        public T PeekRight()
        {
            return _right.PeekRight();
        }
        #endregion

        #region Dequelette Nested Type
        private abstract class Dequelette
        {
            #region Properties and Indexers
            public abstract int Size { get; }

            public virtual bool Full
            {
                get { return false; }
            }
            #endregion

            #region Methods
            public abstract Dequelette DequeueLeft();
            public abstract Dequelette DequeueRight();
            public abstract Dequelette EnqueueLeft(T t);
            public abstract Dequelette EnqueueRight(T t);
            public abstract T PeekLeft();
            public abstract T PeekRight();
            #endregion
        }
        #endregion

        #region EmptyDeque Nested Type
        private sealed class EmptyDeque : IDeque<T>
        {
            #region IDeque<T> Members
            public bool IsEmpty
            {
                get { return true; }
            }

            public IDeque<T> EnqueueLeft(T value)
            {
                return new SingleDeque(value);
            }

            public IDeque<T> EnqueueRight(T value)
            {
                return new SingleDeque(value);
            }

            public IDeque<T> DequeueLeft()
            {
                throw new InvalidOperationException("empty deque");
            }

            public IDeque<T> DequeueRight()
            {
                throw new InvalidOperationException("empty deque");
            }

            public T PeekLeft()
            {
                throw new InvalidOperationException("empty deque");
            }

            public T PeekRight()
            {
                throw new InvalidOperationException("empty deque");
            }
            #endregion

            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion

        #region Four Nested Type
        private class Four : Dequelette
        {
            #region Fields
            private readonly T _v1;
            private readonly T _v2;
            private readonly T _v3;
            private readonly T _v4;
            #endregion

            #region Constructors
            public Four(T t1, T t2, T t3, T t4)
            {
                _v1 = t1;
                _v2 = t2;
                _v3 = t3;
                _v4 = t4;
            }
            #endregion

            #region Properties and Indexers
            public override int Size
            {
                get { return 4; }
            }

            public override bool Full
            {
                get { return true; }
            }
            #endregion

            #region Methods
            public override Dequelette DequeueLeft()
            {
                return new Three(_v2, _v3, _v4);
            }

            public override Dequelette DequeueRight()
            {
                return new Three(_v1, _v2, _v3);
            }

            public override Dequelette EnqueueLeft(T t)
            {
                throw new InvalidOperationException("Impossible");
            }

            public override Dequelette EnqueueRight(T t)
            {
                throw new InvalidOperationException("Impossible");
            }

            public override T PeekLeft()
            {
                return _v1;
            }

            public override T PeekRight()
            {
                return _v4;
            }
            #endregion
        }
        #endregion

        #region One Nested Type
        private class One : Dequelette
        {
            #region Fields
            private readonly T _v1;
            #endregion

            #region Constructors
            public One(T t1)
            {
                _v1 = t1;
            }
            #endregion

            #region Properties and Indexers
            public override int Size
            {
                get { return 1; }
            }
            #endregion

            #region Methods
            public override Dequelette DequeueLeft()
            {
                throw new InvalidOperationException("Impossible");
            }

            public override Dequelette DequeueRight()
            {
                throw new InvalidOperationException("Impossible");
            }

            public override Dequelette EnqueueLeft(T t)
            {
                return new Two(t, _v1);
            }

            public override Dequelette EnqueueRight(T t)
            {
                return new Two(_v1, t);
            }

            public override T PeekLeft()
            {
                return _v1;
            }

            public override T PeekRight()
            {
                return _v1;
            }
            #endregion
        }
        #endregion

        #region SingleDeque Nested Type
        private sealed class SingleDeque : IDeque<T>
        {
            #region Fields
            private readonly T _item;
            #endregion

            #region Constructors
            public SingleDeque(T t)
            {
                _item = t;
            }
            #endregion

            #region IDeque<T> Members
            public bool IsEmpty
            {
                get { return false; }
            }

            public IDeque<T> EnqueueLeft(T value)
            {
                return new ImmutableDeque<T>(new One(value), ImmutableDeque<Dequelette>.Empty, new One(_item));
            }

            public IDeque<T> EnqueueRight(T value)
            {
                return new ImmutableDeque<T>(new One(_item), ImmutableDeque<Dequelette>.Empty, new One(value));
            }

            public IDeque<T> DequeueLeft()
            {
                return Empty;
            }

            public IDeque<T> DequeueRight()
            {
                return Empty;
            }

            public T PeekLeft()
            {
                return _item;
            }

            public T PeekRight()
            {
                return _item;
            }
            #endregion

            public IEnumerator<T> GetEnumerator()
            {
                yield return _item;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion

        #region Three Nested Type
        private class Three : Dequelette
        {
            #region Fields
            private readonly T _v1;
            private readonly T _v2;
            private readonly T _v3;
            #endregion

            #region Constructors
            public Three(T t1, T t2, T t3)
            {
                _v1 = t1;
                _v2 = t2;
                _v3 = t3;
            }
            #endregion

            #region Properties and Indexers
            public override int Size
            {
                get { return 3; }
            }
            #endregion

            #region Methods
            public override Dequelette DequeueLeft()
            {
                return new Two(_v2, _v3);
            }

            public override Dequelette DequeueRight()
            {
                return new Two(_v1, _v2);
            }

            public override Dequelette EnqueueLeft(T t)
            {
                return new Four(t, _v1, _v2, _v3);
            }

            public override Dequelette EnqueueRight(T t)
            {
                return new Four(_v1, _v2, _v3, t);
            }

            public override T PeekLeft()
            {
                return _v1;
            }

            public override T PeekRight()
            {
                return _v3;
            }
            #endregion
        }
        #endregion

        #region Two Nested Type
        private class Two : Dequelette
        {
            #region Fields
            private readonly T _v1;
            private readonly T _v2;
            #endregion

            #region Constructors
            public Two(T t1, T t2)
            {
                _v1 = t1;
                _v2 = t2;
            }
            #endregion

            #region Properties and Indexers
            public override int Size
            {
                get { return 2; }
            }
            #endregion

            #region Methods
            public override Dequelette DequeueLeft()
            {
                return new One(_v2);
            }

            public override Dequelette DequeueRight()
            {
                return new One(_v1);
            }

            public override Dequelette EnqueueLeft(T t)
            {
                return new Three(t, _v1, _v2);
            }

            public override Dequelette EnqueueRight(T t)
            {
                return new Three(_v1, _v2, t);
            }

            public override T PeekLeft()
            {
                return _v1;
            }

            public override T PeekRight()
            {
                return _v2;
            }
            #endregion
        }
        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            if (IsEmpty)
                yield break;
            yield return PeekLeft();
            foreach (var item in DequeueLeft())
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}