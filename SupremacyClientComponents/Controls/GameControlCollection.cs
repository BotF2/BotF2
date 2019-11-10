using System;
using System.Collections.Specialized;
using System.Windows;

namespace Supremacy.Client.Controls
{
    public class GameControlCollection<T> : DeferrableObservableCollection<T> where T : DependencyObject
    {
        private readonly GameControlContext _context;
        private VariantSize? _itemVariantSize;
        private readonly object _owner;

        public GameControlCollection() { }

        public GameControlCollection(object owner, GameControlContext context)
            : this()
        {
            _owner = owner;
            _context = context;
        }

        public GameControlCollection(object owner, GameControlContext context, VariantSize? itemVariantSize)
            : this(owner, context)
        {
            _itemVariantSize = itemVariantSize;
        }

        public void AddRange(T[] items)
        {
            foreach (var item in items)
                Add(item);
        }

        protected override void ClearItems()
        {
            var itemArray = new T[Count];

            Items.CopyTo(itemArray, 0);
            base.ClearItems();

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    itemArray));
        }

        public bool Contains(string label)
        {
            return (IndexOf(label) != -1);
        }

        public GameControlContext Context
        {
            get { return _context; }
        }

        public int IndexOf(string label)
        {
            for (var index = 0; index < Count; index++)
            {
                if (GameControlService.GetLabel(this[index]) == label)
                    return index;
            }
            return -1;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var logicalParent = _owner as ILogicalParent;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (var newItem in e.NewItems)
                    {
                        var variantControl = newItem as IVariantControl;
                        if (variantControl != null)
                        {
                            if (_context != GameControlContext.None)
                                variantControl.Context = _context;
                            if (_itemVariantSize.HasValue)
                                variantControl.VariantSize = _itemVariantSize.Value;
                        }
                        if (logicalParent != null)
                            logicalParent.AddLogicalChild(newItem);
                    }
                    break;
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                {
                    if (logicalParent != null)
                    {
                        foreach (var oldItem in e.OldItems)
                            logicalParent.RemoveLogicalChild(oldItem);
                    }
                    break;
                }
            }

            try
            {
                base.OnCollectionChanged(e);
            }
            catch (ArgumentOutOfRangeException) { }
        }

        public object Owner
        {
            get { return _owner; }
        }

        public T this[string label]
        {
            get
            {
                var index = IndexOf(label);
                return (index != -1) ? this[index] : null;
            }
        }
    }
}