// ObservableCollectionExtended.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Supremacy.Collections
{
    [Serializable]
    public class ObservableCollectionExtended<T> : ObservableCollection<T>, ISupportInitialize
    {
        #region Fields
        private bool _isInitializing;
        #endregion

        #region Method Overrides
        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
        }
        #endregion

        #region Helper Methods
        protected void VerifyInitializing()
        {
            if (!_isInitializing)
                throw new InvalidOperationException("Collection can only be modified during initialization.");
        }
        #endregion

        #region Implementation of ISupportInitialize
        public void BeginInit()
        {
            _isInitializing = true;
        }

        public void EndInit()
        {
            _isInitializing = false;
        }
        #endregion
    }
}