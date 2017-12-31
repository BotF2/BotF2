using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class TypeName : NameExpression
    {
        private int _genericArity = -1;

        public TypeName() {}

        public TypeName(BuiltinType builtinType)
            : this()
        {
            IsBuiltinType = true;
            Name = builtinType.ToString();
        }

        public TypeName(Type type)
            : this()
        {
            Name = type.Name;
        }

        [DefaultValue(false)]
        public bool IsGenericTypeDefinition { get; set; }

        [DefaultValue(false)]
        public bool IsBuiltinType { get; set; }

        [DefaultValue(0)]
        public int GenericArity
        {
            get { return _genericArity + 1; }
            set { _genericArity = value; }
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            var name = Name;

            if (IsBuiltinType)
            {
                var builtinType = TypeManager.GetClrTypeFromBuiltinType(
                    (BuiltinType)Enum.Parse(typeof(BuiltinType), 
                    Name));

                if (builtinType != null)
                    name = TypeManager.GetCSharpName(builtinType);
            }

            sw.Write(name);

            if (!HasTypeArguments && !IsGenericTypeDefinition)
                return;

            sw.Write("<");

            if (IsGenericTypeDefinition)
            {
                for (int i = 0; i < GenericArity - 1; i++)
                    sw.Write(",");
            }
            else
            {
                for (int i = 0; i < TypeArguments.Count; i++)
                {
                    var typeArgument = TypeArguments[i];

                    if (i != 0)
                        sw.Write(", ");

                    typeArgument.Dump(sw, indentChange);
                }
            }

            sw.Write(">");
        }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            if (IsBuiltinType)
                return new TypeExpression(TypeManager.GetClrTypeFromBuiltinType((BuiltinType)Enum.Parse(typeof(BuiltinType), Name)));

            return base.DoResolve(parseContext);
        }
    }

    public class EnumerableToCountConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(IEnumerable).IsAssignableFrom(sourceType))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var enumerable = value as IEnumerable;
            if (enumerable != null)
                return enumerable.Cast<object>().Count() + 1;
            return base.ConvertFrom(context, culture, value);
        }
    }
}