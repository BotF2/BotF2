using System;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class NullableTypeExpression : TypeExpression
    {
        private FullNamedExpression _underlyingType;

        public FullNamedExpression UnderlyingType
        {
            get { return _underlyingType; }
            set
            {
                _underlyingType = value;
                if ((_underlyingType.Type != null) && _underlyingType.Type.IsValueType)
                    Type = TypeManager.CoreTypes.GenericNullable.MakeGenericType(_underlyingType.Type);
            }
        }

        public NullableTypeExpression(TypeExpression underlyingType, SourceSpan span)
            : this()
        {
            _underlyingType = underlyingType;
            Span = span;
        }

        public NullableTypeExpression()
        {
            ExpressionClass = ExpressionClass.Type;
        }

        public NullableTypeExpression(Type type, SourceSpan loc)
            : this(new TypeExpression(type, loc), loc) { }

        protected override TypeExpression DoResolveAsTypeStep(ParseContext ec)
        {
            var args = new TypeArguments(_underlyingType);
            var ctype = new GenericTypeExpression(
                TypeManager.CoreTypes.GenericNullable,
                args,
                Span);
            return ctype.ResolveAsTypeTerminal(ec, false);
        }

        public override TypeExpression ResolveAsTypeTerminal(ParseContext ec, bool silent)
        {
            return ResolveAsBaseTerminal(ec, silent);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            DumpChild(_underlyingType, sw);
            sw.Write("?");
        }
    }
}