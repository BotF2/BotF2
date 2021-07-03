// Initializable.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;

using Supremacy.Annotations;

namespace Supremacy.Types
{
    [Serializable]
    public abstract class SupportInitializeBase : ISupportInitializeNotification, INotifyPropertyChanged
    {
        private bool _isInitialized;
        private bool _isInitializing;

        private readonly object _syncRoot;

        protected SupportInitializeBase()
            : this(null) { }

        protected SupportInitializeBase(object syncRoot)
        {
            _syncRoot = syncRoot ?? new object();
        }

        public object SyncRoot => _syncRoot;

        protected T Synchronize<T>([NotNull] Func<T> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            lock (SyncRoot)
            {
                return function();
            }
        }

        protected void Synchronize([NotNull] Action function)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            lock (SyncRoot)
            {
                function();
            }
        }

        protected virtual void BeginInitCore() { }
        protected virtual void EndInitCore() { }

        protected void VerifyInitializing()
        {
            if (_isInitializing)
            {
                return;
            }

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            throw new InvalidOperationException("This operation can only be performed during initialization.");
        }

        public void EnsureInitialized()
        {
            if (IsInitialized)
            {
                return;
            }

            if (!IsInitializing)
            {
                BeginInit();
            }

            EndInit();
        }

        #region Implementation of ISupportInitialize
        public void BeginInit()
        {
            lock (SyncRoot)
            {
                _isInitialized = false;
                _isInitializing = true;

                BeginInitCore();
            }
        }

        public void EndInit()
        {
            lock (SyncRoot)
            {
                if (_isInitialized)
                {
                    return;
                }

                try
                {
                    EndInitCore();
                }
                finally
                {
                    _isInitialized = true;
                    _isInitializing = false;
                }

                OnInitialized();
            }
        }
        #endregion

        #region Implementation of ISupportInitializeNotification
        public bool IsInitialized
        {
            get
            {
                lock (SyncRoot)
                {
                    return _isInitialized;
                }
            }
        }

        public bool IsInitializing
        {
            get
            {
                lock (SyncRoot)
                {
                    return _isInitializing;
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler Initialized;

        private void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChangedEventHandler previousValue = _propertyChanged;

                while (true)
                {
                    PropertyChangedEventHandler combinedValue = (PropertyChangedEventHandler)Delegate.Combine(previousValue, value);

                    PropertyChangedEventHandler valueBeforeCombine = System.Threading.Interlocked.CompareExchange(
                        ref _propertyChanged,
                        combinedValue,
                        previousValue);

                    if (previousValue == valueBeforeCombine)
                    {
                        return;
                    }
                }
            }
            remove
            {
                PropertyChangedEventHandler previousValue = _propertyChanged;

                while (true)
                {
                    PropertyChangedEventHandler removedValue = (PropertyChangedEventHandler)Delegate.Remove(previousValue, value);

                    PropertyChangedEventHandler valueBeforeRemove = System.Threading.Interlocked.CompareExchange(
                        ref _propertyChanged,
                        removedValue,
                        previousValue);

                    if (previousValue == valueBeforeRemove)
                    {
                        return;
                    }
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}