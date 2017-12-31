// FreezableBase.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Utility;

namespace Supremacy.Types
{
    public abstract class FreezableBase : SupportInitializeBase, INotifyChanged
    {
        private static readonly EventDescriptor ChangedEvent = TypeDescriptor.GetEvents(typeof(INotifyChanged))["Changed"];

        private bool _isFrozen;
        private bool _isInitializing;
        [NonSerialized] private HashSet<object> _children;
        [NonSerialized] private DelegatingWeakEventListener<EventHandler> _weakChildChangedHandler;

        protected FreezableBase() {}
        protected FreezableBase(object syncRoot) : base(syncRoot) {}

        public bool IsFrozen
        {
            get
            {
                lock (SyncRoot)
                {
                    return _isFrozen;
                }
            }
        }

        public bool CanFreeze
        {
            get
            {
                lock (SyncRoot)
                {
                    return !_isFrozen && !_isInitializing && CanFreezeCore;
                }
            }
        }

        protected virtual bool CanFreezeCore
        {
            get { return true; }
        }

        public void Freeze()
        {
            lock (SyncRoot)
            {
                if (!CanFreeze)
                    return;

                try
                {
                    FreezeCore();
                }
                finally
                {
                    _isFrozen = true;
                }
            }
        }

        protected void AddChild([NotNull] object child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            lock (SyncRoot)
            {
                if (_children == null)
                    _children = new HashSet<object>();

                if (!_children.Add(child))
                    return;

                if (_weakChildChangedHandler == null)
                    _weakChildChangedHandler = new DelegatingWeakEventListener<EventHandler>(OnChildChanged);

                OnChanged();
            }

            var lockAcquired = false;
            var freezableChild = child as FreezableBase;
            if (freezableChild != null)
            {
                lockAcquired = true;
                Monitor.Enter(freezableChild.SyncRoot);
            }

            try
            {
                WeakEventHelper.AddListener(
                    child,
                    ChangedEvent,
                    _weakChildChangedHandler);
            }
            finally
            {
                if (lockAcquired)
                    Monitor.Exit(freezableChild.SyncRoot);
            }
        }

        protected void RemoveChild([NotNull] object child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            lock (SyncRoot)
            {
                if (_children == null)
                    return;

                if (!_children.Remove(child))
                    return;

                OnChanged();

                if (_weakChildChangedHandler == null)
                    return;
            }

            var lockAcquired = false;
            var freezableChild = child as FreezableBase;
            if (freezableChild != null)
            {
                lockAcquired = true;
                Monitor.Enter(freezableChild.SyncRoot);
            }

            try 
            {
                WeakEventHelper.RemoveListener(
                    child,
                    ChangedEvent,
                    _weakChildChangedHandler);
            }
            finally
            {
                if (lockAcquired)
                    Monitor.Exit(freezableChild.SyncRoot);
            }
        }

        private void OnChildChanged(object sender, EventArgs e)
        {
            OnChanged();
        }

        protected virtual void FreezeCore() {}

        public FreezableBase CreateInstance()
        {
            return CreateInstanceCore(null);
        }

        public FreezableBase Clone()
        {
            lock (SyncRoot)
            {
                var clone = CreateInstance();
                Synchronize(() => clone.CloneCore(this));
                return clone;
            }
        }

        protected virtual void CloneCore(FreezableBase sourceFreezable)
        {
            CloneCoreCommon(
                sourceFreezable,
                /* useCurrentValue = */ false,
                /* cloneFrozenValues = */ true);
        } 

        public FreezableBase CloneCurrentValue()
        {
            var clone = CreateInstance();
            Synchronize(() => clone.CloneCurrentValueCore(this));
            return clone;
        }

        protected virtual void CloneCurrentValueCore(FreezableBase sourceFreezable)
        {
            CloneCoreCommon(
                sourceFreezable,
                /* useCurrentValue = */ true,
                /* cloneFrozenValues = */ true);
        }

        public FreezableBase GetAsFrozen()
        {
            lock (SyncRoot)
            {
                if (IsFrozen)
                    return this;

                var clone = CreateInstanceCore(SyncRoot);

                clone.GetAsFrozenCore(this);
                clone.Freeze();

                return clone;
            }
        }

        public FreezableBase GetCurrentValueAsFrozen()
        {
            lock (SyncRoot)
            {
                if (IsFrozen)
                    return this;

                var clone = CreateInstanceCore(SyncRoot);

                clone.GetCurrentValueAsFrozenCore(this);
                clone.Freeze();

                return clone;
            }
        }

        protected virtual void GetCurrentValueAsFrozenCore(FreezableBase sourceFreezable)
        {
            CloneCoreCommon(
                sourceFreezable,
                /* useCurrentValue = */ true,
                /* cloneFrozenValues = */ false);
        } 

        protected virtual void GetAsFrozenCore(FreezableBase sourceFreezable)
        {
            CloneCoreCommon(
                sourceFreezable,
                /* useCurrentValue = */ false,
                /* cloneFrozenValues = */ false);
        }

        protected abstract FreezableBase CreateInstanceCore(object syncRoot);

        protected virtual void CloneCoreCommon(FreezableBase sourceFreezable, bool useCurrentValue, bool cloneFrozenValues) {}

        /// <summary>
        /// Verifies that this instance is unfrozen.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this instance is frozen. </exception>
        protected void VerifyUnfrozen()
        {
            lock (SyncRoot)
            {
                if (!_isFrozen)
                    return;
            }
            throw new InvalidOperationException("A frozen object cannot be modified.");
        }

        protected sealed override void BeginInitCore()
        {
            // Synchronization guaranteed by BeginInit()
            VerifyUnfrozen();
            _isInitializing = true;
            BeginInitOverride();
        }

        protected sealed override void EndInitCore()
        {
            // Synchronization guaranteed by EndInit()
            try
            {
                BeginInitOverride();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        protected virtual void BeginInitOverride() {}
        protected virtual void EndInitOverride() {}

        #region Implementation of INotifyChanged
        public event EventHandler Changed;

        protected virtual void OnChanged()
        {
            lock (SyncRoot)
            {
                var handler = Changed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}