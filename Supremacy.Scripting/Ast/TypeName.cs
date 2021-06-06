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

        public TypeName() { }

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
            get => _genericArity + 1;
            set => _genericArity = value;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            string name = Name;

            if (IsBuiltinType)
            {
                Type builtinType = TypeManager.GetClrTypeFromBuiltinType(
                    (BuiltinType)Enum.Parse(typeof(BuiltinType),
                    Name));

                if (builtinType != null)
                {
                    name = TypeManager.GetCSharpName(builtinType);
                }
            }

            sw.Write(name);

            if (!HasTypeArguments && !IsGenericTypeDefinition)
            {
                return;
            }

            sw.Write("<");

            if (IsGenericTypeDefinition)
            {
                for (int i = 0; i < GenericArity - 1; i++)
                {
                    sw.Write(",");
                }
            }
            else
            {
                for (int i = 0; i < TypeArguments.Count; i++)
                {
                    FullNamedExpression typeArgument = TypeArguments[i];

                    if (i != 0)
                    {
                        sw.Write(", ");
                    }

                    typeArgument.Dump(sw, indentChange);
                }
            }

            sw.Write(">");
        }

        public override bool IsPrimaryExpression => true;

        public override Expression DoResolve(ParseContext parseContext)
        {
            return IsBuiltinType
                ? new TypeExpression(TypeManager.GetClrTypeFromBuiltinType((BuiltinType)Enum.Parse(typeof(BuiltinType), Name)))
                : base.DoResolve(parseContext);
        }
    }

    public class EnumerableToCountConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return typeof(IEnumerable).IsAssignableFrom(sourceType) ? true : base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value is IEnumerable enumerable ? enumerable.Cast<object>().Count() + 1 : base.ConvertFrom(context, culture, value);
        }
    }
}