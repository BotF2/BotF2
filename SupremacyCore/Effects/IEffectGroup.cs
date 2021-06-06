using System;
using System.Collections.Generic;

using Obtics.Values;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Scripting;

using System.Linq;

using Supremacy.Scripting.Utility;

namespace Supremacy.Effects
{
    [Serializable]
    public sealed class EffectGroup
    {
        private readonly Type _sourceType;
        private readonly IEffectActivation _activation;
        private readonly IEffectScope _scope;
        private readonly IIndexedCollection<Effect> _effects;
        private readonly IEffectParameterBindingCollection _customParameterBindings;
        private readonly Lazy<EffectParameter> _sourceParameter;
        private readonly Lazy<EffectParameter> _targetParameter;
        private readonly Lazy<ScriptParameters> _customScriptParameters;
        private readonly Lazy<RuntimeScriptParameters> _customRuntimeScriptParameters;
        private readonly Lazy<ScriptExpression> _activationScript;
        private readonly Lazy<ScriptExpression> _scopeScript;
        private readonly Lazy<string> _activationDescription;
        private readonly Lazy<string> _scopeDescription;
        private readonly Lazy<ScriptParameter> _sourceScriptParameter;
        private readonly Lazy<ScriptParameter> _targetScriptParameter;
        private readonly Lazy<ScriptParameters> _systemScriptParameters;
        private readonly Lazy<EffectParameterCollection> _systemParameters;

        private EffectGroup()
        {
            _sourceParameter = new Lazy<EffectParameter>(CreateSourceParameter);
            _sourceScriptParameter = new Lazy<ScriptParameter>(() => _sourceParameter.Value.ToScriptParameter());
            _targetParameter = new Lazy<EffectParameter>(CreateTargetParameter);
            _targetScriptParameter = new Lazy<ScriptParameter>(() => _targetParameter.Value.ToScriptParameter());
            _customScriptParameters = new Lazy<ScriptParameters>(CreateCustomScriptParameters);
            _customRuntimeScriptParameters = new Lazy<RuntimeScriptParameters>(CreateCustomRuntimeScriptParameters);

            _activationScript = new Lazy<ScriptExpression>(EvaluateActivation);
            _scopeScript = new Lazy<ScriptExpression>(EvaluateScope);
            _activationDescription = new Lazy<string>(EvaluateActivationDescription);
            _scopeDescription = new Lazy<string>(EvaluateScopeDescription);

            _systemParameters = new Lazy<EffectParameterCollection>(
                () => new EffectParameterCollection
                      {
                          _sourceParameter.Value,
                          _targetParameter.Value
                      });

            _systemScriptParameters = new Lazy<ScriptParameters>(
                () => new ScriptParameters(_sourceScriptParameter.Value, _targetScriptParameter.Value));
        }

        private EffectGroup(
            [NotNull] Type sourceType,
            [CanBeNull] IEffectActivation activation,
            [NotNull] IEffectScope scope,
            [NotNull] IIndexedCollection<Effect> effects,
            [CanBeNull] IEffectParameterBindingCollection customParameterBindings)
            : this()
        {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");
            if (scope == null)
                throw new ArgumentNullException("scope");
            if (effects == null)
                throw new ArgumentNullException("effects");

            if (!typeof(IEffectSource).IsAssignableFrom(sourceType))
            {
                throw new ArgumentException(
                    string.Format(
                        "Source type must implement '{0}'.",
                        typeof(IEffectSource).FullName),
                    "sourceType");
            }

            _sourceType = sourceType;
            _activation = activation;
            _scope = scope;
            _effects = effects;
            _customParameterBindings = customParameterBindings ?? new EffectParameterBindingCollection();

            _effects.ForEach(o => o.EffectGroup = this);
        }

        internal EffectParameterCollection SystemParameters => _systemParameters.Value;

        internal ScriptParameter SourceScriptParameter => _sourceScriptParameter.Value;

        internal ScriptParameters SystemScriptParameters => _systemScriptParameters.Value;

        public IEffectActivation Activation => _activation;

        public IEffectScope Scope => _scope;

        public IIndexedCollection<Effect> Effects => _effects;

        public IEffectParameterBindingCollection CustomParameterBindings => _customParameterBindings;

        internal ScriptParameters CustomScriptParameters => _customScriptParameters.Value;

        internal RuntimeScriptParameters CustomRuntimeScriptParameters => _customRuntimeScriptParameters.Value;

        internal ScriptExpression ActivationScript => _activationScript.Value;

        internal ScriptExpression ScopeScript => _scopeScript.Value;

        internal string ActivationDescription => _activationDescription.Value;

        internal string ScopeDescription => _scopeDescription.Value;

        public EffectGroupBinding Bind(IEffectSource source)
        {
            return new EffectGroupBinding(
                source,
                this,
                CustomParameterBindings);
        }

        private ScriptExpression EvaluateActivation()
        {
            if (_activation == null)
                return null;

            return new ScriptExpression
            {
                ScriptCode = _activation.ValueExpression,
                Parameters = new ScriptParameters(SourceScriptParameter)
            };
        }

        private ScriptExpression EvaluateScope()
        {
            return new ScriptExpression
            {
                ScriptCode = _scope.ValueExpression,
                Parameters = new ScriptParameters(_sourceScriptParameter.Value)
            };
        }

        private string EvaluateActivationDescription()
        {
            if (_activation == null || string.IsNullOrWhiteSpace(_activation.DescriptionExpression))
            {
                return null;
            }

            ScriptExpression script = new ScriptExpression
            {
                ScriptCode = _activation.DescriptionExpression,
                Parameters = new ScriptParameters(CustomScriptParameters)
            };

            object result = script.Evaluate(CustomRuntimeScriptParameters);

            return result is IValueProvider<string> stringValueProvider
                ? stringValueProvider.Value
                : result is IValueProvider valueProvider ? valueProvider.Value as string : result as string;
        }

        private string EvaluateScopeDescription()
        {
            if (string.IsNullOrWhiteSpace(_scope.DescriptionExpression))
            {
                return null;
            }

            ScriptExpression script = new ScriptExpression
            {
                ScriptCode = _scope.DescriptionExpression,
                Parameters = new ScriptParameters(CustomScriptParameters)
            };

            object result = script.Evaluate(CustomRuntimeScriptParameters);

            return result is IValueProvider<string> stringValueProvider
                ? stringValueProvider.Value
                : result is IValueProvider valueProvider ? valueProvider.Value as string : result as string;
        }

        private EffectParameter CreateTargetParameter()
        {
            Type targetType = ScopeScript.ReturnType;

            if (targetType.IsGenericType)
            {
                Type openType = TypeManager.DropGenericTypeArguments(targetType);
                if (typeof(IValueProvider<>).IsAssignableFrom(openType))
                    targetType = targetType.GetGenericArguments()[0];
            }

            if (targetType.IsGenericType)
            {
                Type openType = TypeManager.DropGenericTypeArguments(targetType);
                if (typeof(IEnumerable<>).IsAssignableFrom(openType))
                    targetType = targetType.GetGenericArguments()[0];
            }

            return new EffectParameter(Effect.ParameterNameTarget, targetType);
        }

        private EffectParameter CreateSourceParameter()
        {
            return new EffectParameter(Effect.ParameterNameSource, _sourceType);
        }

        private ScriptParameters CreateCustomScriptParameters()
        {
            return new ScriptParameters(CustomParameterBindings.Select(o => o.Parameter.ToScriptParameter()));
        }

        private RuntimeScriptParameters CreateCustomRuntimeScriptParameters()
        {
            return CustomParameterBindings.ToRuntimeScriptParameters(CustomScriptParameters);
        }

        public static EffectGroup Create(
            Type sourceType,
            IEffectActivation activation,
            IEffectScope scope,
            IIndexedCollection<Effect> effects,
            IEffectParameterBindingCollection customParameters)
        {
            return new EffectGroup(sourceType, activation, scope, effects, customParameters);
        }
    }
}