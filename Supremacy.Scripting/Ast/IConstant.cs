using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;

using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public interface IConstant
    {
        void CheckObsoleteness(ParseContext parseContext, SourceSpan location);
        bool ResolveValue();
        ConstantExpression CreateConstantReference(SourceSpan location);
    }

    public class ExternalConstant : IConstant
    {
        private readonly FieldInfo _field;
        private object _value;

        public ExternalConstant(FieldInfo field)
        {
            _field = field;
        }

        private ExternalConstant(FieldInfo field, object value) :
            this(field)
        {
            _value = value;
        }

        //
        // Decimal constants cannot be encoded in the constant blob, and thus are marked
        // as IsInitOnly ('readonly' in C# parlance).  We get its value from the 
        // DecimalConstantAttribute metadata.
        //
        public static IConstant CreateDecimal(FieldInfo fi)
        {
            if (fi is FieldBuilder)
            {
                return null;
            }

            DecimalConstantAttribute decimalConstantAttribute =
                fi.GetCustomAttributes(TypeManager.PredefinedAttributes.DecimalConstant, false)
                    .Cast<DecimalConstantAttribute>()
                    .FirstOrDefault();

            return decimalConstantAttribute == null
                ? null
                : new ExternalConstant(
                fi,
                decimalConstantAttribute.Value);
        }

        #region IConstant Members

        public void CheckObsoleteness(ParseContext parseContext, SourceSpan location)
        {
            System.ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute(_field);
            if (oa == null)
            {
                return;
            }

            AttributeTester.ReportObsoleteMessage(parseContext, oa, TypeManager.GetFullNameSignature(_field), location);
        }

        public bool ResolveValue()
        {
            if (_value != null)
            {
                return true;
            }

            _value = _field.GetValue(_field);
            return true;
        }

        public ConstantExpression CreateConstantReference(SourceSpan location)
        {
            return ConstantExpression.Create(_field.FieldType, _value, location);
        }

        #endregion
    }

}