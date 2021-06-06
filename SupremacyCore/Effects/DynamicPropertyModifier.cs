// DynamicPropertyModifier.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;
using Supremacy.Types;

namespace Supremacy.Effects
{
    public abstract class DynamicPropertyModifier<TValue>
    {
        private readonly object _stackingKey;
        private readonly StateScope _invalidationSuppressionScope;

        protected virtual string Description => null;

        protected DynamicPropertyModifier()
        {
            _invalidationSuppressionScope = new StateScope();
        }

        protected DynamicPropertyModifier([NotNull] object stackingKey)
            : this()
        {
            if (stackingKey == null)
                throw new ArgumentNullException("stackingKey");
            _stackingKey = stackingKey;
        }

        [CanBeNull]
        public object StackingKey => _stackingKey;

        public TValue ProvideValue(TValue baseValue, TValue currentValue)
        {
            return ProvideValueCore(baseValue, currentValue);
        }

        protected abstract TValue ProvideValueCore(TValue baseValue, TValue currentValue);

        internal IDisposable SuppressInvalidations()
        {
            return _invalidationSuppressionScope.Enter();
        }

        public void Invalidate()
        {
            if (_invalidationSuppressionScope.IsWithin)
                return;
            OnInvalidated();
        }

        public event EventHandler Invalidated;

        private void OnInvalidated()
        {
            EventHandler handler = Invalidated;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}