using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Supremacy.Types;

namespace Supremacy.Collections
{
    public class EditableKeyedCollectionBase<TKey, TValue> : KeyedCollectionBase<TKey, TValue>, IEditableObject, IRevertibleChangeTracking
    {
        private readonly bool _trackItemChanges;
        private readonly List<NotifyCollectionChangedEventArgs> _changeLog;
        private readonly StateScope _suppressChangeLoggingScope;

        private bool _isEditInProgress;

        public EditableKeyedCollectionBase(Func<TValue, TKey> keyRetriever, bool trackItemChanges) : base(keyRetriever)
        {
            _trackItemChanges = trackItemChanges;
            _changeLog = new List<NotifyCollectionChangedEventArgs>();
            _suppressChangeLoggingScope = new StateScope();
            base.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_isEditInProgress)
                return;
            if (_suppressChangeLoggingScope.IsWithin)
                return;
            _changeLog.Add(e);
        }

        #region Implementation of IEditableObject
        public void BeginEdit()
        {
            if (_trackItemChanges)
            {
                base.SyncLock.EnterReadLock();
                try
                {
                    foreach (var value in this.OfType<IEditableObject>())
                        value.BeginEdit();
                }
                finally
                {
                    base.SyncLock.ExitReadLock();
                }
            }
            _isEditInProgress = true;
        }

        public void EndEdit()
        {
            AcceptChanges();
            if (_trackItemChanges)
            {
                base.SyncLock.EnterReadLock();
                try
                {
                    foreach (var value in this.OfType<IEditableObject>())
                        value.EndEdit();
                }
                finally
                {
                    base.SyncLock.ExitReadLock();
                }
            }
            _isEditInProgress = false;
        }

        public void CancelEdit()
        {
            RejectChanges();
            if (_trackItemChanges)
            {
                base.SyncLock.EnterReadLock();
                try
                {
                    foreach (var value in this.OfType<IEditableObject>())
                        value.CancelEdit();
                }
                finally
                {
                    base.SyncLock.ExitReadLock();
                }
            }
            _isEditInProgress = false;
        }
        #endregion

        #region Implementation of IChangeTracking
        public void AcceptChanges()
        {
            base.SyncLock.EnterReadLock();
            try
            {
                using (_suppressChangeLoggingScope.Enter())
                {

                    if (_trackItemChanges)
                    {
                        foreach (var value in this.OfType<IChangeTracking>())
                            value.AcceptChanges();
                    }
                    _changeLog.Clear();
                }
            }
            finally
            {
                base.SyncLock.ExitReadLock();
            }
        }

        public bool IsChanged
        {
            get { return _isEditInProgress && _changeLog.Any(); }
        }

        public void RejectChanges()
        {
            base.SyncLock.EnterWriteLock();
            try
            {
                using (_suppressChangeLoggingScope.Enter())
                {
                    if (_trackItemChanges)
                    {
                        foreach (var value in this.OfType<IRevertibleChangeTracking>())
                            value.RejectChanges();
                    }
                    foreach (var change in _changeLog)
                    {
                        switch (change.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                base.RemoveAt(change.NewStartingIndex);
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                base.InsertItem(change.OldStartingIndex, change.OldItems.OfType<TValue>().Single());
                                break;

                            case NotifyCollectionChangedAction.Reset:
                                base.AddRange(change.NewItems.OfType<TValue>());
                                break;
                        }
                        _changeLog.Clear();
                    }
                }
            }
            finally
            {
                base.SyncLock.ExitWriteLock();
            }
        }
        #endregion
    }
}