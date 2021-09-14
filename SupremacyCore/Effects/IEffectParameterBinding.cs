using System;

namespace Supremacy.Effects
{
    public interface IEffectParameterBinding
    {
        IEffectParameter Parameter { get; }
        object Value { get; }
    }

    [Serializable]
    internal class EffectParameterBinding : IEffectParameterBinding
    {
        private readonly IEffectParameter _parameter;
        private readonly object _value;

        public EffectParameterBinding(IEffectParameter parameter, object value)
        {
            _parameter = parameter;
            _value = value;
        }

        #region Implementation of IEffectParameterBinding

        public IEffectParameter Parameter => _parameter;

        public object Value => _value;

        #endregion
    }
}