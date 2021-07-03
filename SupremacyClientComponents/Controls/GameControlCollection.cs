using System;
using System.Collections.Specialized;
using System.Windows;

namespace Supremacy.Client.Controls
{
    public class GameControlCollection<T> : DeferrableObservableCollection<T> where T : DependencyObject
    {
        private VariantSize? _itemVariantSize;

        public GameControlCollection() { }

        public GameControlCollection(object owner, GameControlContext context)
            : this()
        {
            Owner = owner;
            Context = context;
        }

        public GameControlCollection(object owner, GameControlContext context, VariantSize? itemVariantSize)
            : this(owner, context)
        {
            _itemVariantSize = itemVariantSize;
        }

        public void AddRange(T[] items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        protected override void ClearItems()
        {
            T[] itemArray = new T[Count];

            Items.CopyTo(itemArray, 0);
            base.ClearItems();

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    itemArray));
        }

        public bool Contains(string label)
        {
            return IndexOf(label) != -1;
        }

        public GameControlContext Context { get; }

        public int IndexOf(string label)
        {
            for (int index = 0; index < Count; index++)
            {
                if (GameControlService.GetLabel(this[index]) == label)
                {
                    return index;
                }
            }
            return -1;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ILogicalParent logicalParent = Owner as ILogicalParent;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (object newItem in e.NewItems)
                        {
                            if (newItem is IVariantControl variantControl)
                            {
                                if (Context != GameControlContext.None)
                                {
                                    variantControl.Context = Context;
                                }

                                if (_itemVariantSize.HasValue)
                                {
                                    variantControl.VariantSize = _itemVariantSize.Value;
                                }
                            }
                            if (logicalParent != null)
                            {
                                logicalParent.AddLogicalChild(newItem);
                            }
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
                            foreach (object oldItem in e.OldItems)
                            {
                                logicalParent.RemoveLogicalChild(oldItem);
                            }
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

        public object Owner { get; }

        public T this[string label]
        {
            get
            {
                int index = IndexOf(label);
                return (index != -1) ? this[index] : null;
            }
        }
    }
}