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
            get { return _name; }
            set
            {
                if (value == _name)
                    return;
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
                    if (_typeName == null)
                        Type = typeof(object);
                    else
                        Type = Type.GetType(_typeName);
                }
                return _type;
            }
            set
            {
                if (value == null)
                    value = typeof(object);

                _type = value;
                _typeName = value.FullName + ", " + value.Assembly.GetName().Name;

                OnPropertyChanged("Type");

                var defaultValue = DefaultValue;

                if (!IsValidValue(ref defaultValue))
                    defaultValue = _type.IsValueType ? Activator.CreateInstance(_type) : null;

                DefaultValue = defaultValue;
            }
        }

        public bool IsValidValue(ref object baseValue)
        {
            var type = Type;

            if (baseValue == null)
                return !type.IsValueType;

            var sourceType = baseValue.GetType();

            if (type.IsAssignableFrom(sourceType))
                return true;

            var targetConverter = TypeDescriptor.GetConverter(type);

            if (targetConverter.CanConvertFrom(sourceType))
            {
                //try
                {
                    var convertedValue = targetConverter.ConvertFrom(baseValue);
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
                    var convertedValue = targetConverter.ConvertTo(baseValue, type);
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
            get { return _isRequired; }
            set
            {
                if (value == _isRequired)
                    return;
                _isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        [DefaultValue(null)]
        [ConstructorArgument("defaultValue")]
        public object DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }

        private void VerifyValid()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Parameter must have a valid Name.");

            var defaultValue = DefaultValue;
            if (!IsValidValue(ref defaultValue))
            {
                if (IsRequired)
                    throw new InvalidOperationException("");
            }
        }

        public bool Equals(ScriptParameter other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Name, Name) && Equals(other.Type, Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(ScriptParameter))
                return false;
            return Equals((ScriptParameter)obj);
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
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
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
        public bool IsInitialized
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler Initialized;

        public void OnInitialized()
        {
            var handler = Initialized;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion
    }
}