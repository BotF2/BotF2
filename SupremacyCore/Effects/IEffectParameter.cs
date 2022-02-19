using System;

using Supremacy.Annotations;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Effects
{
    public interface IEffectParameter
    {
        string Name { get; }
        Type ParameterType { get; }
        bool IsRequired { get; }
        object DefaultValue { get; }
    }

    [Serializable]
    public class EffectParameter : SupportInitializeBase, IEffectParameter
    {
        private string _name;
        private Type _parameterType;
        private bool _isRequired;
        private object _defaultValue;

        public EffectParameter() { }

        public EffectParameter(
            [NotNull] string name,
            [NotNull] Type parameterType,
            bool isRequired = true,
            object defaultValue = null)
        {
            _name = name ?? throw new ArgumentNullException("name");
            _parameterType = parameterType ?? throw new ArgumentNullException("parameterType");
            _isRequired = isRequired;
            _defaultValue = defaultValue;
        }

        public string Name
        {
            get => _name;
            set
            {
                _ = Guard.ArgumentNotNullOrWhiteSpace(value, "value");
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public Type ParameterType
        {
            get => _parameterType;
            set
            {
                _ = Guard.ArgumentNotNull(value, "value");
                _parameterType = value;
                OnPropertyChanged("ParameterType");
            }
        }

        public bool IsRequired
        {
            get => _isRequired;
            set
            {
                _isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        public object DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }
    }
}