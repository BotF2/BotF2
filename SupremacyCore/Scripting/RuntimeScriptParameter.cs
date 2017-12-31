using System;
using System.Windows.Markup;

namespace Supremacy.Scripting
{
    [Serializable]
    public sealed class RuntimeScriptParameter
    {
        private readonly ScriptParameter _parameter;
        private readonly object _value;

        public RuntimeScriptParameter(ScriptParameter parameter, object value)
        {
            _parameter = parameter;

            var valueIsNull = (value == null);

            if (!_parameter.IsValidValue(ref value))
            {
                var parameterTypeName = _parameter.Type.Name;

                if (valueIsNull)
                {
                    throw new ArgumentException(
                        string.Format(
                            "'null' is not a valid value for parameter of type '{0}'.",
                            parameterTypeName),
                        "value");
                }

                throw new ArgumentException(
                    string.Format(
                        "'{0}' is not a valid value for parameter of type '{1}'.",
                        value.GetType().Name,
                        parameterTypeName),
                    "value");
            }

            _value = value;
        }

        [ConstructorArgument("parameter")]
        public ScriptParameter Parameter
        {
            get { return _parameter; }
        }

        [ConstructorArgument("value")]
        public object Value
        {
            get { return _value; }
        }
    }
}