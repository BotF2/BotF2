using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;

namespace Supremacy.Effects
{
    public class DynamicPropertyValue<TValue> : INotifyPropertyChanged
    {
        [Flags]
        protected enum Flags : byte
        {
            IsCoerced = 0x01,
            IsComputed = 0x02,
            IsUpdating = 0x04,
            HasModifiers = 0x08,
            IsCoercedWithCurrentValue = 0x10,
            UsesCoercion = 0x20
        }

        protected static readonly List<DynamicPropertyModifier<TValue>> EmptyModifiers = new List<DynamicPropertyModifier<TValue>>();

        private bool _isValid;
        private Flags _flags;
        private object _value;
        private object _currentValue = DynamicProperty.UnsetValue;

        private readonly DynamicProperty<TValue> _property;
        private readonly DynamicObject _sourceObject;
        private readonly DynamicPropertyMetadata<TValue> _metadata;
        private List<DynamicPropertyModifier<TValue>> _modifiers;

        public DynamicProperty<TValue> Property => _property;

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

        internal DynamicPropertyValue([NotNull] DynamicProperty<TValue> property, [NotNull] DynamicObject sourceObject)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            if (sourceObject == null)
                throw new ArgumentNullException("sourceObject");

            _property = property;
            _sourceObject = sourceObject;
            _metadata = property.GetMetadata(sourceObject.DynamicObjectType);
            _value = _metadata.DefaultValue;
            _modifiers = EmptyModifiers;

            UsesCoercion = (_metadata.CoerceValueCallback != null);
        }

        internal bool IsCoerced
        {
            get { return GetFlag(Flags.IsCoerced); }
            private set
            {
                SetFlag(Flags.IsCoerced, value);

                if (!value)
                    IsCoercedWithComputedValue = false;
            }
        }

        internal bool IsUpdating
        {
            get { return GetFlag(Flags.IsUpdating); }
            private set { SetFlag(Flags.IsUpdating, value); }
        }

        internal bool IsComputed
        {
            get { return GetFlag(Flags.IsComputed); }
            private set { SetFlag(Flags.IsComputed, value); }
        }

        internal bool HasModifiers
        {
            get { return GetFlag(Flags.HasModifiers); }
            private set { SetFlag(Flags.HasModifiers, value); }
        }

        internal bool UsesCoercion
        {
            get { return GetFlag(Flags.UsesCoercion); }
            private set { SetFlag(Flags.UsesCoercion, value); }
        }

        internal object Value
        {
            get { return _value; }
            private set { _value = value; }
        }

        internal ModifiedValue<TValue> ModifiedValue => _value as ModifiedValue<TValue>;

        internal void ResetCoercedValue(bool updateValue)
        {
            lock (DynamicProperty.Synchronized)
            {
                _isValid = false;

                if (IsCoerced)
                {
                    IsCoerced = false;
                    IsCoercedWithComputedValue = false;

                    if (!HasModifiers)
                    {
                        IsComputed = false;
                        Value = BaseValue;
                    }
                    else if (ModifiedValue != null)
                    {
                        ModifiedValue.CoercedValue = default(TValue);
                    }
                }

                if (updateValue)
                    UpdateValue();
            }
        }

        internal void ResetComputedValue()
        {
            lock (DynamicProperty.Synchronized)
            {
                _isValid = false;

                if (IsComputed)
                {
                    IsComputed = false;
                    IsCoercedWithComputedValue = false;

                    if (UsesCoercion)
                        ResetCoercedValue(false);
                    else if (!HasModifiers)
                        Value = BaseValue;

                    if (ModifiedValue != null)
                        ModifiedValue.ComputedValue = default(TValue);
                }

                UpdateValue();
            }
        }

        internal void AddModifier([NotNull] DynamicPropertyModifier<TValue> modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException("modifier");

            lock (DynamicProperty.Synchronized)
            {
                if (_modifiers == EmptyModifiers)
                    _modifiers = new List<DynamicPropertyModifier<TValue>>();

                _modifiers.Add(modifier);
                modifier.Invalidated += OnModifierInvalidated;

                HasModifiers = true;

                if (IsUpdating)
                    return;

                _isValid = false;

                ResetComputedValue();
            }
        }

        private void OnModifierInvalidated(object sender, EventArgs e)
        {
            lock (DynamicProperty.Synchronized)
            {
                if (IsUpdating)
                    return;
                if (IsComputed)
                    ResetComputedValue();
            }
        }

        internal void RemoveModifier([NotNull] DynamicPropertyModifier<TValue> modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException("modifier");

            lock (DynamicProperty.Synchronized)
            {
                if ((_modifiers == EmptyModifiers) || !_modifiers.Remove(modifier))
                    return;

                modifier.Invalidated -= OnModifierInvalidated;
                HasModifiers = (_modifiers.Count > 0);

                if (IsUpdating)
                    return;

                _isValid = false;

                ResetComputedValue();
            }
        }

        private ModifiedValue<TValue> EnsureModifiedValue()
        {
            var modifiedValue = _value as ModifiedValue<TValue>;
            if (modifiedValue == null)
            {
                if (_value == null)
                {
                    _value = modifiedValue = new ModifiedValue<TValue>();
                }
                else
                {
                    modifiedValue = _value as ModifiedValue<TValue>;
                    if (modifiedValue == null)
                    {
                        modifiedValue = new ModifiedValue<TValue> { BaseValue = (TValue)_value };
                        _value = modifiedValue;
                    }
                }
            }
            return modifiedValue;
        }

        internal void ClearValue()
        {
            ResetCore(DynamicProperty.UnsetValue);
        }

        internal void Reset(TValue baseValue)
        {
            lock (DynamicProperty.Synchronized)
            {
                if (DynamicProperty<TValue>.ValuesEqual(baseValue, BaseValue))
                    return;

                ResetCore(baseValue);
            }
        }

        private void ResetCore(object baseValue)
        {
            lock (DynamicProperty.Synchronized)
            {
                VerifyNotUpdating();

                _value = baseValue;

                IsCoerced = false;
                IsCoercedWithComputedValue = false;
                IsComputed = false;

                _isValid = false;

                OnBaseValueChanged();

                //if (this.HasModifiers)
                //    this.OnComputedValueChanged();

                UpdateValue();
            }
        }

        private void UpdateValue()
        {
            lock (DynamicProperty.Synchronized)
            {
                if (_isValid)
                    return;

                var lastCurrentValue = (_currentValue == DynamicProperty.UnsetValue) ? _metadata.DefaultValue : _currentValue;

                _currentValue = EnsureValue();
                _isValid = true;

                if (Equals(lastCurrentValue, _currentValue))
                    return;

                OnComputedValueChanged();

                var changeNotificationArgs = new DynamicPropertyChangedEventArgs<TValue>(
                    _property,
                    (TValue)lastCurrentValue,
                    (TValue)_currentValue);

                PublishChangeNotifications(changeNotificationArgs);

                return;
            }
        }

        private void PublishChangeNotifications(DynamicPropertyChangedEventArgs<TValue> args)
        {
            var propertyChangedCallback = _metadata.PropertyChangedCallback;
            if (propertyChangedCallback == null)
                return;

            propertyChangedCallback(
                _sourceObject,
                args);
        }

        internal bool IsCoercedWithComputedValue
        {
            get { return GetFlag(Flags.IsCoercedWithCurrentValue); }
            set { SetFlag(Flags.IsCoercedWithCurrentValue, value); }
        }

        internal void SetCoercedValue(TValue value, bool skipBaseValueChecks, bool coerceWithComputedValue)
        {
            EnsureModifiedValue().CoercedValue = value;
            
            IsCoerced = true;
            
            if (coerceWithComputedValue)
                IsCoercedWithComputedValue = true;
        }

        private void VerifyNotUpdating()
        {
            if (!IsUpdating)
                return;
            throw new InvalidOperationException(
                "DynamicProperty modifiers may not modify the property's base value.");
        }

        private TValue PerformCoercion(TValue baseValue)
        {
            if (!UsesCoercion)
                return baseValue;

            bool revertToBaseValue;
            var newValue = _metadata.CoerceValueCallback(_sourceObject, baseValue, out revertToBaseValue);

            if (revertToBaseValue)
                return baseValue;

            return newValue;
        }

        private TValue EnsureValue()
        {
            if (_isValid)
            {
                if (IsCoerced)
                {
                    Debug.Assert(ModifiedValue != null);
                    return ModifiedValue.CoercedValue;
                }
                
                if (IsComputed)
                {
                    Debug.Assert(ModifiedValue != null);
                    return ModifiedValue.ComputedValue;
                }

                return BaseValue;
            }

            var effectiveValue = BaseValue;

            IsUpdating = true;

            try
            {
                if (HasModifiers)
                {
                    if (!IsComputed)
                    {
                        IsCoerced = false;
                        IsCoercedWithComputedValue = false;

                        var baseValue = BaseValue;
                        var computedValue = baseValue;

                        for (var originalModifiers = new HashSet<DynamicPropertyModifier<TValue>>(_modifiers);
                             /*!_modifiers.SetEquals(originalModifiers)*/;
                             originalModifiers = new HashSet<DynamicPropertyModifier<TValue>>(_modifiers),
                             computedValue = baseValue)
                        {
                            computedValue = _modifiers.Aggregate(
                                computedValue, (current, t) => t.ProvideValue(baseValue, current));

                            if (_modifiers.SetEquals(originalModifiers))
                                break;
                        }

                        effectiveValue = EnsureModifiedValue().ComputedValue = computedValue;

                        IsComputed = true;
                    }
                    else
                    {
                        Debug.Assert(ModifiedValue != null);
                        effectiveValue = ModifiedValue.ComputedValue;
                    }
                }

                if (UsesCoercion && !IsCoerced)
                {
                    effectiveValue = EnsureModifiedValue().CoercedValue = PerformCoercion(effectiveValue);

                    IsCoerced = true;

                    if (IsComputed)
                        IsCoercedWithComputedValue = true;
                }

                return effectiveValue;
            }
            finally
            {
                IsUpdating = false;
            }
        }

        #region BaseValue Property
        public event EventHandler BaseValueChanged;

        public TValue BaseValue
        {
            get
            {
                lock (DynamicProperty.Synchronized)
                {
                    if (_value == DynamicProperty.UnsetValue)
                        return _metadata.DefaultValue;

                    var modifiedValue = _value as ModifiedValue<TValue>;
                    if (modifiedValue != null)
                        return modifiedValue.BaseValue;

                    return (TValue)_value;
                }
            }
            set
            {
                if (Equals(value, BaseValue))
                    return;

                ResetCore(value);
            }
        }

        protected void OnBaseValueChanged()
        {
            var handler = BaseValueChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            OnPropertyChanged("BaseValue");
        }
        #endregion

        #region ComputedValue Property
        public event EventHandler CurrentValueChanged;

        public TValue CurrentValue
        {
            get
            {
                lock (DynamicProperty.Synchronized)
                {
                    if (_value == DynamicProperty.UnsetValue)
                        return _metadata.DefaultValue;
                    if (!_isValid)
                        UpdateValue();
                    return (TValue)_currentValue;
                }
            }
        }

        protected void OnComputedValueChanged()
        {
            var handler = CurrentValueChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            OnPropertyChanged("ComputedValue");
        }
        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerializedAttribute]
        private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                var previousValue = _propertyChanged;

                while (true)
                {
                    var combinedValue = (PropertyChangedEventHandler)Delegate.Combine(previousValue, value);

                    var valueBeforeCombine = System.Threading.Interlocked.CompareExchange(
                        ref _propertyChanged,
                        combinedValue,
                        previousValue);

                    if (previousValue == valueBeforeCombine)
                        return;
                }
            }
            remove
            {
                var previousValue = _propertyChanged;

                while (true)
                {
                    var removedValue = (PropertyChangedEventHandler)Delegate.Remove(previousValue, value);

                    var valueBeforeRemove = System.Threading.Interlocked.CompareExchange(
                        ref _propertyChanged,
                        removedValue,
                        previousValue);

                    if (previousValue == valueBeforeRemove)
                        return;
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = _propertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}