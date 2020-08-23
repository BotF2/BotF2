using System;
using System.ComponentModel;
using System.Windows.Markup;

namespace Supremacy.Scripting
{
    [Serializable]
    public sealed class ScriptParameter : IEquatable<ScriptParameter>, INotifyPropertyChanged, ISupportInitializeNotification
    {
        private string _name;
        private string _typeName;
        private object _defaultValue;
        private bool _isRequired;

        [NonSerialized] private Type _type;

        public ScriptParameter(string name, Type type = null, bool isRequired = true, object defaultValue = null)
        {
            Name = name;
            Type = type;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
        }

        [DefaultValue(null)]
        [ConstructorArgument("name")]
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [DefaultValue(typeof(object))]
        [ConstructorArgument("type")]
        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    Type = _typeName == null ? typeof(object) : Type.GetType(_typeName);
                }
                return _type;
            }
            set
            {
                if (value == null)
                {
                    value = typeof(object);
                }

                _type = value;
                _typeName = value.FullName + ", " + value.Assembly.GetName().Name;

                OnPropertyChanged("Type");

                object defaultValue = DefaultValue;

                if (!IsValidValue(ref defaultValue))
                {
                    defaultValue = _type.IsValueType ? Activator.CreateInstance(_type) : null;
                }

                DefaultValue = defaultValue;
            }
        }

        public bool IsValidValue(ref object baseValue)
        {
            Type type = Type;

            if (baseValue == null)
            {
                return !type.IsValueType;
            }

            Type sourceType = baseValue.GetType();

            if (type.IsAssignableFrom(sourceType))
            {
                return true;
            }

            TypeConverter targetConverter = TypeDescriptor.GetConverter(type);

            if (targetConverter.CanConvertFrom(sourceType))
            {
                //try
                {
                    object convertedValue = targetConverter.ConvertFrom(baseValue);
                    baseValue = convertedValue;
                    return true;
                }
                //catch { } //ToDo: think how to add Supremacy.Utility namespace here, maybe move utility into seperate sub-project?
            }

            targetConverter = TypeDescriptor.GetConverter(sourceType);

            if (targetConverter.CanConvertTo(type))
            {
                //try
                {
                    object convertedValue = targetConverter.ConvertTo(baseValue, type);
                    baseValue = convertedValue;
                    return true;
                }
                //catch { } //ToDo: think how to add Supremacy.Utility namespace here, maybe move utility into seperate sub-project?
            }

            return false;
        }

        [DefaultValue(true)]
        [ConstructorArgument("isRequired")]
        public bool IsRequired
        {
            get => _isRequired;
            set
            {
                if (value == _isRequired)
                {
                    return;
                }

                _isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        [DefaultValue(null)]
        [ConstructorArgument("defaultValue")]
        public object DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }

        private void VerifyValid()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidOperationException("Parameter must have a valid Name.");
            }

            object defaultValue = DefaultValue;
            if (!IsValidValue(ref defaultValue))
            {
                if (IsRequired)
                {
                    throw new InvalidOperationException("");
                }
            }
        }

        public bool Equals(ScriptParameter other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other) ? true : Equals(other.Name, Name) && Equals(other.Type, Type);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() != typeof(ScriptParameter) ? false : Equals((ScriptParameter)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ Type.GetHashCode();
            }
        }

        #region Implementation of INotifyPropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Implementation of ISupportInitialize
        public void BeginInit() { }

        public void EndInit()
        {
            VerifyValid();
            OnInitialized();
        }
        #endregion

        #region Implementation of ISupportInitializeNotification
        public bool IsInitialized => throw new NotImplementedException();

        public event EventHandler Initialized;

        public void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}