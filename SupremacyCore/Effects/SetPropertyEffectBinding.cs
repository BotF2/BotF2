using System;
using System.ComponentModel;

using Obtics.Values;

using Supremacy.Annotations;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Effects
{
    [Serializable]
    internal sealed class SetPropertyEffectBinding<TValue>
        : EffectBinding<SetPropertyEffectBinding<TValue>.ParameterBindingContext>
    {
        private readonly DynamicProperty<TValue> _targetProperty;
        private readonly EffectPropertyModifier _propertyModifier;

        public SetPropertyEffectBinding(
            [NotNull] SetPropertyEffect effect,
            [NotNull] EffectGroupBinding effectGroupBinding,
            [NotNull] IEffectTarget target)
            : base(effect, effectGroupBinding, Guard.ArgumentAssignableAs<IEffectTarget, DynamicObject>(target, "target"))
        {
            _targetProperty = (DynamicProperty<TValue>)effect.TargetProperty;
            _propertyModifier = new EffectPropertyModifier(this);
        }

        public new SetPropertyEffect Effect => (SetPropertyEffect)base.Effect;

        public new DynamicObject Target => (DynamicObject)base.Target;

        protected override void AttachCore()
        {
            Target.GetValue(_targetProperty).AddModifier(_propertyModifier);
        }

        protected override void DetachCore()
        {
            Target.GetValue(_targetProperty).RemoveModifier(_propertyModifier);
        }

        protected override IDisposable SuspendCore()
        {
            return _propertyModifier.SuppressInvalidations();
        }

        protected override void UpdateTargetCore()
        {
            _propertyModifier.UpdateTarget();
        }

        protected override IEffectParameterBindingCollection BindSystemParameters(ParameterBindingContext bindingContext)
        {
            EffectParameterBinding[] additionalBindings = new[]
                                     {
                                         new EffectParameterBinding(
                                             Effect.SystemParameters[SetPropertyEffect.ParameterNameBaseValue],
                                             bindingContext.BaseValue),

                                         new EffectParameterBinding(
                                             Effect.SystemParameters[SetPropertyEffect.ParameterNameCurrentValue],
                                             bindingContext.CurrentValue)
                                     };

            return new EffectParameterBindingCollection(
                base.BindSystemParameters(bindingContext).Concat(additionalBindings));
        }

        internal sealed class ParameterBindingContext
        {
            private readonly TValue _baseValue;
            private readonly TValue _currentValue;

            public ParameterBindingContext(TValue baseValue, TValue currentValue)
            {
                _baseValue = baseValue;
                _currentValue = currentValue;
            }

            public TValue BaseValue => _baseValue;

            public TValue CurrentValue => _currentValue;
        }

        #region Nested Class: EffectPropertyModifier

        [Serializable]
        private class EffectPropertyModifier : DynamicPropertyModifier<TValue>
        {
            private readonly SetPropertyEffectBinding<TValue> _effectBinding;

            private IValueProvider<TValue> _currentValueProvider;

            internal EffectPropertyModifier([NotNull] SetPropertyEffectBinding<TValue> effectBinding)
            {
                if (effectBinding == null)
                    throw new ArgumentNullException("effectBinding");

                _effectBinding = effectBinding;
            }

            internal void UpdateTarget()
            {
                lock (EffectSystem.SyncRoot)
                {
                    if (EffectSystem.IsSuspended)
                        return;

                    Invalidate();
                }
            }

            protected override string Description => _effectBinding.Description ?? base.Description;

            protected override TValue ProvideValueCore(TValue baseValue, TValue currentValue)
            {
                if (_currentValueProvider != null)
                    ((INotifyPropertyChanged)_currentValueProvider).PropertyChanged -= OnObservableValueChanged;

                ParameterBindingContext context = new ParameterBindingContext(baseValue, currentValue);
                Scripting.RuntimeScriptParameters mergedRuntimeScriptParameters = _effectBinding.GetMergedRuntimeScriptParameters(context);

                _currentValueProvider = _effectBinding.Effect.ValueScript.Evaluate<IValueProvider<TValue>>(
                    mergedRuntimeScriptParameters);

                ((INotifyPropertyChanged)_currentValueProvider).PropertyChanged += OnObservableValueChanged;

                return _currentValueProvider.Value;
            }

            private void OnObservableValueChanged(object sender, PropertyChangedEventArgs e)
            {
                UpdateTarget();
            }
        }

        #endregion
    }
}