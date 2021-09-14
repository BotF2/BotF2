using System;
using System.Collections.Generic;
using System.ComponentModel;

using Supremacy.Annotations;
using Supremacy.Collections;

using System.Linq;

using Supremacy.Scripting;
using Supremacy.Utility;

namespace Supremacy.Effects
{
    [Serializable]
    public sealed class EffectGroupBinding : INotifyPropertyChanged
    {
        private readonly IEffectSource _source;
        private readonly EffectGroup _effectGroup;
        private readonly IEffectParameterBindingCollection _customParameterBindings;
        private readonly KeyedCollectionBase<IEffectTarget, TargetEffectBinding> _targetEffectBindings;

        internal EffectGroupBinding(
            [NotNull] IEffectSource source,
            [NotNull] EffectGroup effectGroup,
            IEffectParameterBindingCollection customParameterBindings)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _effectGroup = effectGroup ?? throw new ArgumentNullException("effectGroup");
            _customParameterBindings = customParameterBindings;
            _targetEffectBindings = new KeyedCollectionBase<IEffectTarget, TargetEffectBinding>(o => o.Target);
        }

        public IEffectSource Source => _source;

        public EffectGroup EffectGroup => _effectGroup;

        public IEffectParameterBindingCollection CustomParameterBindings => _customParameterBindings;

        public IEnumerable<EffectBinding> EffectBindings => _targetEffectBindings.SelectMany(o => o.EffectBindings);

        public IIndexedCollection<EffectBinding> GetEffectBindings(IEffectTarget target)
        {

            if (_targetEffectBindings.TryGetValue(target, out TargetEffectBinding targetEffectBinding))
            {
                return targetEffectBinding.EffectBindings;
            }

            return ArrayWrapper<EffectBinding>.Empty;
        }

        internal void AttachTarget([NotNull] IEffectTarget effectTarget)
        {
            if (effectTarget == null)
            {
                throw new ArgumentNullException("effectTarget");
            }

            lock (EffectSystem.SyncRoot)
            {

                if (_targetEffectBindings.TryGetValue(effectTarget, out TargetEffectBinding targetEffectBinding))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "EffectGroup is already attached to target '{0}'.",
                            effectTarget));
                }

                IIndexedCollection<Effect> effects = EffectGroup.Effects;
                EffectBinding[] effectBindings = new EffectBinding[effects.Count];

                effects.Select(o => o.Bind(this, effectTarget)).CopyTo(effectBindings);

                TargetEffectBinding targetBindings = new TargetEffectBinding(
                    effectTarget,
                    new ArrayWrapper<EffectBinding>(effectBindings));

                _targetEffectBindings.Add(targetBindings);

                if (effectTarget is IEffectTargetInternal internalTarget)
                {
                    internalTarget.EffectBindingsInternal.AddRange(effectBindings);
                }

                _ = effectBindings.ForEach(o => o.Attach());
            }
        }

        internal void DetachTarget([NotNull] IEffectTarget effectTarget)
        {
            if (effectTarget == null)
            {
                throw new ArgumentNullException("effectTarget");
            }

            lock (EffectSystem.SyncRoot)
            {
                if (!_targetEffectBindings.TryGetValue(effectTarget, out TargetEffectBinding targetEffectBinding))
                {
                    return;
                }

                foreach (EffectBinding effectBinding in targetEffectBinding.EffectBindings)
                {
                    try
                    {
                        effectBinding.Detach();

                        if (effectTarget is IEffectTargetInternal internalTarget)
                        {
                            _ = internalTarget.EffectBindingsInternal.Remove(effectBinding);
                        }
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(e);
                    }
                }

                _ = _targetEffectBindings.Remove(effectTarget);
            }
        }

        internal RuntimeScriptParameters BindActivationScriptRuntimeParameters()
        {
            RuntimeScriptParameters parameters = new RuntimeScriptParameters
                             {
                                 new RuntimeScriptParameter(EffectGroup.SourceScriptParameter, Source)
                             };

            parameters.AddRange(CustomParameterBindings.ToRuntimeScriptParameters(EffectGroup.CustomScriptParameters));

            return parameters;
        }

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

        [UsedImplicitly]
        private void OnPropertyChanged(string propertyName)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Nested Class: TargetEffectBinding

        [Serializable]
        private class TargetEffectBinding
        {
            private readonly IEffectTarget _target;
            private readonly IIndexedCollection<EffectBinding> _effectBindings;

            public TargetEffectBinding([NotNull] IEffectTarget target, [NotNull] IIndexedCollection<EffectBinding> effectBindings)
            {
                _target = target ?? throw new ArgumentNullException("target");
                _effectBindings = effectBindings ?? throw new ArgumentNullException("effectBindings");
            }

            public IEffectTarget Target => _target;

            public IIndexedCollection<EffectBinding> EffectBindings => _effectBindings;
        }

        #endregion
    }
}