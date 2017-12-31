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

        public EffectParameter() {}

        public EffectParameter(
            [NotNull] string name,
            [NotNull] Type parameterType,
            bool isRequired = true,
            object defaultValue = null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (parameterType == null)
                throw new ArgumentNullException("parameterType");

            _name = name;
            _parameterType = parameterType;
            _isRequired = isRequired;
            _defaultValue = defaultValue;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                Guard.ArgumentNotNullOrWhiteSpace(value, "value");
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public Type ParameterType
        {
            get { return _parameterType; }
            set
            {
                Guard.ArgumentNotNull(value, "value");
                _parameterType = value;
                OnPropertyChanged("ParameterType");
            }
        }

        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                _isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        public object DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }
    }
}