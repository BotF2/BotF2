using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Collections
{
    public interface IBinaryTree<V>
    {
        #region Properties and Indexers
        bool IsEmpty { get; }
        V Value { get; }
        IBinaryTree<V> Left { get; }
        IBinaryTree<V> Right { get; }
        #endregion
    }

    public interface IMap<K, V>
        where K : IComparable<K>
    {
        #region Properties and Indexers
        IEnumerable<K> Keys { get; }
        IEnumerable<V> Values { get; }
        IEnumerable<KeyValuePair<K, V>> Pairs { get; }
        #endregion

        #region Methods
        IMap<K, V> Add(K key, V value);
        bool Contains(K key);
        V Lookup(K key);
        IMap<K, V> Remove(K key);
        #endregion
    }

    public interface IBinarySearchTree<K, V> :
        IBinaryTree<V>,
        IMap<K, V>
        where K : IComparable<K>
    {
        #region Properties and Indexers
        K Key { get; }
        new IBinarySearchTree<K, V> Left { get; }
        new IBinarySearchTree<K, V> Right { get; }
        #endregion

        #region Methods
        new IBinarySearchTree<K, V> Add(K key, V value);
        new IBinarySearchTree<K, V> Remove(K key);
        IBinarySearchTree<K, V> Search(K key);
        #endregion
    }

    public sealed class ImmutableAvlTree<K, V> : IBinarySearchTree<K, V>
        where K : IComparable<K>
    {
        #region Fields
        private static readonly EmptyImmutableAvlTree s_empty = new EmptyImmutableAvlTree();
        private readonly int _height;

        private readonly K _key;
        private readonly IBinarySearchTree<K, V> _left;
        private readonly IBinarySearchTree<K, V> _right;
        private readonly V _value;
        #endregion

        #region Constructors
        private ImmutableAvlTree(K key, V value, IBinarySearchTree<K, V> left, IBinarySearchTree<K, V> right)
        {
            _key = key;
            _value = value;
            _left = left;
            _right = right;
            _height = 1 + Math.Max(Height(left), Height(right));
        }
        #endregion

        #region Properties and Indexers
        public static IBinarySearchTree<K, V> Empty
        {
            get { return s_empty; }
        }
        #endregion

        #region Methods
        private static int Balance(IBinarySearchTree<K, V> tree)
        {
            if (tree.IsEmpty)
                return 0;
            return Height(tree.Right) - Height(tree.Left);
        }

        private static IBinarySearchTree<K, V> DoubleLeft(IBinarySearchTree<K, V> tree)
        {
            if (tree.Right.IsEmpty)
                return tree;
            var rotatedRightChild = new ImmutableAvlTree<K, V>(tree.Key, tree.Value, tree.Left, RotateRight(tree.Right));
            return RotateLeft(rotatedRightChild);
        }

        private static IBinarySearchTree<K, V> DoubleRight(IBinarySearchTree<K, V> tree)
        {
            if (tree.Left.IsEmpty)
                return tree;
            var rotatedLeftChild = new ImmutableAvlTree<K, V>(tree.Key, tree.Value, RotateLeft(tree.Left), tree.Right);
            return RotateRight(rotatedLeftChild);
        }

        private IEnumerable<IBinarySearchTree<K, V>> Enumerate()
        {
            IStack<IBinarySearchTree<K, V>> stack = ImmutableStack<IBinarySearchTree<K, V>>.Empty;
            for (IBinarySearchTree<K, V> current = this; !current.IsEmpty || !stack.IsEmpty; current = current.Right)
            {
                while (!current.IsEmpty)
                {
                    stack = stack.Push(current);
                    current = current.Left;
                }
                current = stack.Peek();
                stack = stack.Pop();
                yield return current;
            }
        }

        private static int Height(IBinarySearchTree<K, V> tree)
        {
            if (tree.IsEmpty)
                return 0;
            return ((ImmutableAvlTree<K, V>)tree)._height;
        }

        private static bool IsLeftHeavy(IBinarySearchTree<K, V> tree)
        {
            return Balance(tree) <= -2;
        }

        private static bool IsRightHeavy(IBinarySearchTree<K, V> tree)
        {
            return Balance(tree) >= 2;
        }

        private static IBinarySearchTree<K, V> MakeBalanced(IBinarySearchTree<K, V> tree)
        {
            IBinarySearchTree<K, V> result;
            if (IsRightHeavy(tree))
            {
                if (IsLeftHeavy(tree.Right))
                    result = DoubleLeft(tree);
                else
                    result = RotateLeft(tree);
            }
            else if (IsLeftHeavy(tree))
            {
                if (IsRightHeavy(tree.Left))
                    result = DoubleRight(tree);
                else
                    result = RotateRight(tree);
            }
            else
                result = tree;
            return result;
        }

        private static IBinarySearchTree<K, V> RotateLeft(IBinarySearchTree<K, V> tree)
        {
            if (tree.Right.IsEmpty)
                return tree;
            return new ImmutableAvlTree<K, V>(
                tree.Right.Key,
                tree.Right.Value,
                new ImmutableAvlTree<K, V>(tree.Key, tree.Value, tree.Left, tree.Right.Left),
                tree.Right.Right);
        }

        private static IBinarySearchTree<K, V> RotateRight(IBinarySearchTree<K, V> tree)
        {
            if (tree.Left.IsEmpty)
                return tree;
            return new ImmutableAvlTree<K, V>(
                tree.Left.Key,
                tree.Left.Value,
                tree.Left.Left,
                new ImmutableAvlTree<K, V>(tree.Key, tree.Value, tree.Left.Right, tree.Right));
        }
        #endregion

        #region IBinarySearchTree<K,V> Members
        public bool IsEmpty
        {
            get { return false; }
        }

        public V Value
        {
            get { return _value; }
        }

        IBinaryTree<V> IBinaryTree<V>.Left
        {
            get { return _left; }
        }

        IBinaryTree<V> IBinaryTree<V>.Right
        {
            get { return _right; }
        }

        public IBinarySearchTree<K, V> Left
        {
            get { return _left; }
        }

        public IBinarySearchTree<K, V> Right
        {
            get { return _right; }
        }

        public IBinarySearchTree<K, V> Search(K key)
        {
            int compare = key.CompareTo(Key);
            if (compare == 0)
                return this;
            if (compare > 0)
                return Right.Search(key);
            return Left.Search(key);
        }

        public K Key
        {
            get { return _key; }
        }

        public IBinarySearchTree<K, V> Add(K key, V value)
        {
            ImmutableAvlTree<K, V> result;
            if (key.CompareTo(Key) > 0)
                result = new ImmutableAvlTree<K, V>(Key, Value, Left, Right.Add(key, value));
            else
                result = new ImmutableAvlTree<K, V>(Key, Value, Left.Add(key, value), Right);
            return MakeBalanced(result);
        }

        public IBinarySearchTree<K, V> Remove(K key)
        {
            IBinarySearchTree<K, V> result;
            int compare = key.CompareTo(Key);
            if (compare == 0)
            {
                // We have a match. If this is a leaf, just remove it 
                // by returning Empty.  If we have only one child,
                // replace the node with the child.
                if (Right.IsEmpty && Left.IsEmpty)
                    result = Empty;
                else if (Right.IsEmpty && !Left.IsEmpty)
                    result = Left;
                else if (!Right.IsEmpty && Left.IsEmpty)
                    result = Right;
                else
                {
                    // We have two children. Remove the next-highest node and replace
                    // this node with it.
                    IBinarySearchTree<K, V> successor = Right;
                    while (!successor.Left.IsEmpty)
                    {
                        successor = successor.Left;
                    }
                    result = new ImmutableAvlTree<K, V>(successor.Key, successor.Value, Left, Right.Remove(successor.Key));
                }
            }
            else if (compare < 0)
                result = new ImmutableAvlTree<K, V>(Key, Value, Left.Remove(key), Right);
            else
                result = new ImmutableAvlTree<K, V>(Key, Value, Left, Right.Remove(key));
            return MakeBalanced(result);
        }

        // IMap
        public bool Contains(K key)
        {
            return !Search(key).IsEmpty;
        }

        IMap<K, V> IMap<K, V>.Add(K key, V value)
        {
            return Add(key, value);
        }

        IMap<K, V> IMap<K, V>.Remove(K key)
        {
            return Remove(key);
        }

        public V Lookup(K key)
        {
            IBinarySearchTree<K, V> tree = Search(key);
            if (tree.IsEmpty)
                throw new Exception("not found");
            return tree.Value;
        }

        public IEnumerable<K> Keys
        {
            get { return from t in Enumerate() select t.Key; }
        }

        public IEnumerable<V> Values
        {
            get { return from t in Enumerate() select t.Value; }
        }

        public IEnumerable<KeyValuePair<K, V>> Pairs
        {
            get { return from t in Enumerate() select new KeyValuePair<K, V>(t.Key, t.Value); }
        }
        #endregion

        #region EmptyImmutableTree Nested Type
        private sealed class EmptyImmutableAvlTree : IBinarySearchTree<K, V>
        {
            #region IBinarySearchTree<K,V> Members
            public bool IsEmpty
            {
                get { return true; }
            }

            public V Value
            {
                get { throw new Exception("empty tree"); }
            }

            IBinaryTree<V> IBinaryTree<V>.Left
            {
                get { throw new Exception("empty tree"); }
            }

            IBinaryTree<V> IBinaryTree<V>.Right
            {
                get { throw new Exception("empty tree"); }
            }

            public IBinarySearchTree<K, V> Left
            {
                get { throw new Exception("empty tree"); }
            }

            public IBinarySearchTree<K, V> Right
            {
                get { throw new Exception("empty tree"); }
            }

            public IBinarySearchTree<K, V> Search(K key)
            {
                return this;
            }

            public K Key
            {
                get { throw new Exception("empty tree"); }
            }

            public IBinarySearchTree<K, V> Add(K key, V value)
            {
                return new ImmutableAvlTree<K, V>(key, value, this, this);
            }

            public IBinarySearchTree<K, V> Remove(K key)
            {
                throw new Exception("Cannot remove item that is not in tree.");
            }

            // IMap
            public bool Contains(K key)
            {
                return false;
            }

            public V Lookup(K key)
            {
                throw new Exception("not found");
            }

            IMap<K, V> IMap<K, V>.Add(K key, V value)
            {
                return Add(key, value);
            }

            IMap<K, V> IMap<K, V>.Remove(K key)
            {
                return Remove(key);
            }

            public IEnumerable<K> Keys
            {
                get { yield break; }
            }

            public IEnumerable<V> Values
            {
                get { yield break; }
            }

            public IEnumerable<KeyValuePair<K, V>> Pairs
            {
                get { yield break; }
            }
            #endregion
        }
        #endregion
    }

    public static class BinaryTreeExtensions
    {
        #region Methods
        public static IEnumerable<T> InOrder<T>(this IBinaryTree<T> tree)
        {
            IStack<IBinaryTree<T>> stack = ImmutableStack<IBinaryTree<T>>.Empty;
            for (IBinaryTree<T> current = tree; !current.IsEmpty || !stack.IsEmpty; current = current.Right)
            {
                while (!current.IsEmpty)
                {
                    stack = stack.Push(current);
                    current = current.Left;
                }
                current = stack.Peek();
                stack = stack.Pop();
                yield return current.Value;
            }
        }
        #endregion
    }
}