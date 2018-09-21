using System;
using System.ComponentModel;
using System.Disposables;
using System.Linq;

using Obtics.Values;

using Supremacy.Annotations;
using Supremacy.Scripting;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Effects
{
    [Serializable]
    public abstract class EffectBinding: INotifyPropertyChanged
    {
        private readonly Effect _effect;
        private readonly EffectGroupBinding _effectGroupBinding;
        private readonly IEffectTarget _target;
        private readonly Lazy<ScriptParameters> _systemScriptParameters;
        private readonly Lazy<ScriptParameters> _mergedScriptParameters;
        private readonly StateScope _suspendScope;

        private bool _isAttached;
        private IValueProvider _description;
        private EffectState _state;

        protected EffectBinding(
            [NotNull] Effect effect,
            [NotNull] EffectGroupBinding effectGroupBinding,
            [NotNull] IEffectTarget target)
            : this()
        {
            if (effect == null)
                throw new ArgumentNullException("effect");
            if (effectGroupBinding == null)
                throw new ArgumentNullException("effectGroupBinding");
            if (target == null)
                throw new ArgumentNullException("target");

            _effect = effect;
            _effectGroupBinding = effectGroupBinding;
            _target = target;
        }

        private EffectBinding()
        {
            _suspendScope = new StateScope(UpdateState);
            _systemScriptParameters = new Lazy<ScriptParameters>(() => Effect.SystemParameters.ToScriptParameters());
            _mergedScriptParameters = new Lazy<ScriptParameters>(GetMergedScriptParameters);
        }

        public string Description
        {
            get
            {
                if (_description != null)
                    return _description.Value as string;
                return null;
            }
        }

        public IEffectTarget Target
        {
            get { return _target; }
        }

        public Effect Effect
        {
            get { return _effect; }
        }

        public EffectState State
        {
            get
            {
                lock (EffectSystem.SyncRoot)
                    return _state;
            }
            private set
            {
                lock (EffectSystem.SyncRoot)
                    _state = value;
            }
        }

        public bool IsAttached
        {
            get
            {
                lock (EffectSystem.SyncRoot) 
                    return (_state != EffectState.Detached);
            }
        }

        public bool IsApplied
        {
            get
            {
                lock (EffectSystem.SyncRoot)
                    return (_state == EffectState.Attached);
            }
        }

        public EffectGroupBinding EffectGroupBinding
        {
            get { return _effectGroupBinding; }
        }
        
        protected ScriptParameters SystemScriptParameters
        {
            get { return _systemScriptParameters.Value; }
        }

        protected ScriptParameters MergedScriptParameters
        {
            get { return _mergedScriptParameters.Value; }
        }

        private void UpdateState()
        {
            lock (EffectSystem.SyncRoot)
            {
                if (_suspendScope.IsWithin)
                    State = EffectState.Suspended;
                else if (_isAttached)
                    State = EffectState.Attached;
                else
                    State = EffectState.Detached;
            }
        }

        public IDisposable Suspend()
        {
            lock (EffectSystem.SyncRoot)
            {
                return new CompositeDisposable(
                    _suspendScope.Enter(),
                    SuspendCore());
            }
        }

        protected abstract void AttachCore();
        protected abstract void DetachCore();
        protected abstract IDisposable SuspendCore();
        protected abstract void UpdateTargetCore();

        public void UpdateTarget()
        {
            lock (EffectSystem.SyncRoot)
                UpdateTargetCore();
        }

        public void Attach()
        {
            lock (EffectSystem.SyncRoot)
            {
                if (_isAttached)
                    return;

                GameLog.Core.General.DebugFormat(
                    "Attaching effect to object {{{0}}}: {1}",
                    Target,
                    Description);

                try
                {
                    AttachDescription();
                    AttachCore();
                }
                finally
                {
                    _isAttached = true;
                }
            }
        }

        private void AttachDescription()
        {
            if (_description == null)
            {
                if (!Effect.HasDescription)
                {
                    _description = ValueProvider.Static<string>(null);
                }
                else
                {
                    var descriptionScript = new ScriptExpression
                                            {
                                                ScriptCode = Effect.DescriptionExpression,
                                                Parameters = EffectGroupBinding.EffectGroup.CustomScriptParameters
                                            };

                    var runtimeScriptParameters = EffectGroupBinding.EffectGroup.CustomRuntimeScriptParameters;

                    _description = descriptionScript.Evaluate(runtimeScriptParameters) as IValueProvider;
                }
            }

            var observableDescription = _description as INotifyPropertyChanged;
            if (observableDescription != null)
                observableDescription.PropertyChanged += OnDescriptionPropertyChanged;
        }

        private ScriptParameters GetMergedScriptParameters()
        {
            return SystemScriptParameters.Merge(EffectGroupBinding.EffectGroup.CustomScriptParameters);
        }

        private void DetachDescription()
        {
            var observableDescription = _description as INotifyPropertyChanged;
            if (observableDescription == null)
                return;

            observableDescription.PropertyChanged -= OnDescriptionPropertyChanged;
        }

        private void OnDescriptionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Description");
        }

        public void Detach()
        {
            lock (EffectSystem.SyncRoot)
            {
                if (!_isAttached)
                    return;

                GameLog.Core.General.DebugFormat(
                    "Detaching effect to object {{{0}}}: {1}",
                    Target,
                    Description);

                try
                {
                    DetachCore();
                    DetachDescription();
                }
                finally
                {
                    _isAttached = false;
                }
            }
        }

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

    public abstract class EffectBinding<TParameterBindingContext> : EffectBinding
    {
        protected EffectBinding(
            [NotNull] Effect effect,
            [NotNull] EffectGroupBinding effectGroupBinding,
            [NotNull] IEffectTarget target) : base(effect, effectGroupBinding, target) {}

        protected IEffectParameterBindingCollection GetMergedParameterBindings(TParameterBindingContext bindingContext)
        {
            return new EffectParameterBindingCollection(
                BindSystemParameters(bindingContext).Concat(
                    EffectGroupBinding.CustomParameterBindings));
        }

        protected virtual IEffectParameterBindingCollection BindSystemParameters(TParameterBindingContext bindingContext)
        {
            return new EffectParameterBindingCollection(
                new EffectParameterBinding(
                    EffectGroupBinding.EffectGroup.SystemParameters[Effect.ParameterNameSource],
                    EffectGroupBinding.Source),
                new EffectParameterBinding(
                    EffectGroupBinding.EffectGroup.SystemParameters[Effect.ParameterNameTarget],
                    Target));
        }

        protected RuntimeScriptParameters GetMergedRuntimeScriptParameters(TParameterBindingContext bindingContext)
        {
            return GetMergedParameterBindings(bindingContext).ToRuntimeScriptParameters(MergedScriptParameters);
        }
    }
}