// DynamicPropertyMetadata.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;

namespace Supremacy.Effects
{
    public class DynamicPropertyMetadata<TValue>
    {
        [Flags]
        private enum Flags
        {
            IsDefaultValueModified = 0x01,
            IsSealed = 0x02
        }

        private Flags _flags;
        private TValue _defaultValue;
        private CoerceDynamicPropertyCallback<TValue> _coerceValueCallback;
        private DynamicPropertyChangedCallback<TValue> _propertyChangedCallback;

        private void SetFlag(Flags flag, bool value)
        {
            if (value)
                _flags |= flag;
            else
                _flags &= ~flag;
        }

        private bool GetFlag(Flags flag)
        {
            return ((_flags & flag) == flag);
        }

        public DynamicPropertyMetadata([CanBeNull] TValue defaultValue)
            : this(defaultValue, null, null) { }

        public DynamicPropertyMetadata(
            [CanBeNull] TValue defaultValue,
            [CanBeNull] DynamicPropertyChangedCallback<TValue> propertyChangedCallback)
            : this(defaultValue, propertyChangedCallback, null) { }

        public DynamicPropertyMetadata(
            [CanBeNull] TValue defaultValue,
            [CanBeNull] CoerceDynamicPropertyCallback<TValue> coerceValueCallback)
            : this(defaultValue, null, coerceValueCallback) { }

        public DynamicPropertyMetadata(
            [CanBeNull] TValue defaultValue,
            [CanBeNull] DynamicPropertyChangedCallback<TValue> propertyChangedCallback,
            [CanBeNull] CoerceDynamicPropertyCallback<TValue> coerceValueCallback)
        {
            DefaultValue = defaultValue;
            CoerceValueCallback = coerceValueCallback;
            PropertyChangedCallback = propertyChangedCallback;
        }

        public DynamicPropertyMetadata([CanBeNull] DynamicPropertyChangedCallback<TValue> propertyChangedCallback)
            : this(default(TValue), propertyChangedCallback, null) { }

        public DynamicPropertyMetadata([CanBeNull] CoerceDynamicPropertyCallback<TValue> coerceValueCallback)
            : this(default(TValue), null, coerceValueCallback) { }

        public DynamicPropertyMetadata(
            [CanBeNull] DynamicPropertyChangedCallback<TValue> propertyChangedCallback,
            [CanBeNull] CoerceDynamicPropertyCallback<TValue> coerceValueCallback)
            : this(default(TValue), propertyChangedCallback, coerceValueCallback) { }

        public TValue DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                VerifyUnsealed();
                _defaultValue = value;
            }
        }

        public CoerceDynamicPropertyCallback<TValue> CoerceValueCallback
        {
            get { return _coerceValueCallback; }
            set
            {
                VerifyUnsealed();
                _coerceValueCallback = value;
            }
        }

        public DynamicPropertyChangedCallback<TValue> PropertyChangedCallback
        {
            get { return _propertyChangedCallback; }
            set
            {
                VerifyUnsealed();
                _propertyChangedCallback = value;
            }
        }

        private void VerifyUnsealed()
        {
            if (!IsSealed)
                return;
            throw new InvalidOperationException(
                "Property metadata cannot be modified once it has been used.");
        }

        internal bool IsDefaultValueModified
        {
            get { return GetFlag(Flags.IsDefaultValueModified); }
            set { SetFlag(Flags.IsDefaultValueModified, value); }
        }

        protected internal bool IsSealed
        {
            get { return GetFlag(Flags.IsSealed); }
            internal set { SetFlag(Flags.IsSealed, value); }
        }

        protected virtual void OnApply(DynamicProperty<TValue> property, Type targetType) { }

        internal void Merge(DynamicPropertyMetadata<TValue> baseMetadata, DynamicProperty<TValue> property)
        {
            if (baseMetadata == null)
                throw new ArgumentNullException("baseMetadata");

            if (IsSealed)
                VerifyUnsealed();

            if (!IsDefaultValueModified)
                _defaultValue = baseMetadata.DefaultValue;
            
            if (baseMetadata.PropertyChangedCallback != null)
            {
                var invocationList = baseMetadata.PropertyChangedCallback.GetInvocationList();
                if (invocationList.Length > 0)
                {
                    var target = (DynamicPropertyChangedCallback<TValue>)invocationList[0];
                    for (var i = 1; i < invocationList.Length; i++)
                        target = (DynamicPropertyChangedCallback<TValue>)Delegate.Combine(target, invocationList[i]);
                    target = (DynamicPropertyChangedCallback<TValue>)Delegate.Combine(target, _propertyChangedCallback);
                    _propertyChangedCallback = target;
                }
            }
            
            if (_coerceValueCallback == null)
                _coerceValueCallback = baseMetadata.CoerceValueCallback;
        }

        internal void Seal(DynamicProperty<TValue> property, Type targetType)
        {
            OnApply(property, targetType);
            IsSealed = true;
        }
    }
}