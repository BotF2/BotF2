using System;

using Supremacy.Annotations;
using Supremacy.Scripting;
using Supremacy.Utility;

namespace Supremacy.Effects
{
    [Serializable]
    public sealed class SetPropertyEffect : Effect
    {
        public const string ParameterNameBaseValue = "$baseValue";
        public const string ParameterNameCurrentValue = "$currentValue";

        private readonly DynamicProperty _targetProperty;
        private readonly string _valueExpression;
        private readonly Lazy<ScriptExpression> _valueScript;

        public SetPropertyEffect([NotNull] DynamicProperty targetProperty, [NotNull] string valueExpression)
        {
            Guard.ArgumentNotNull(targetProperty, "targetProperty");
            Guard.ArgumentNotNullOrWhiteSpace(valueExpression, "valueExpression");

            _targetProperty = targetProperty;
            _valueExpression = valueExpression;
            _valueScript = new Lazy<ScriptExpression>(CreateValueScript);
        }

        private ScriptExpression CreateValueScript()
        {
            ScriptParameters mergedParameters = SystemScriptParameters.Merge(EffectGroup.CustomScriptParameters);

            return new ScriptExpression
                   {
                       ScriptCode = _valueExpression,
                       Parameters = mergedParameters
                   };
        }

        public DynamicProperty TargetProperty => _targetProperty;

        public ScriptExpression ValueScript => _valueScript.Value;

        protected override EffectBinding BindCore(EffectGroupBinding effectGroupBinding, IEffectTarget effectTarget)
        {
            Type propertyType = _targetProperty.PropertyType;
            Type bindingType = typeof(SetPropertyEffectBinding<>).MakeGenericType(propertyType);

            return (EffectBinding)Activator.CreateInstance(
                bindingType,
                this,
                effectGroupBinding,
                effectTarget);
        }

        protected override EffectParameterCollection CreateSystemParameters()
        {
            EffectParameterCollection baseParameters = base.CreateSystemParameters();

            baseParameters.Add(new EffectParameter(ParameterNameBaseValue, _targetProperty.PropertyType));
            baseParameters.Add(new EffectParameter(ParameterNameCurrentValue, _targetProperty.PropertyType));

            return baseParameters;
        }
    }
}