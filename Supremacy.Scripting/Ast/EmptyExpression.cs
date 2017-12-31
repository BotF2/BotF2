using System;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class EmptyExpression : Expression
    {
        public static readonly Expression Null = new EmptyExpression();

        public static readonly EmptyExpression OutAccess = new EmptyExpression();
        public static readonly EmptyExpression LValueMemberAccess = new EmptyExpression();
        public static readonly EmptyExpression LValueMemberOutAccess = new EmptyExpression();
        public static readonly EmptyExpression UnaryAddress = new EmptyExpression();

        private static EmptyExpression _temp = new EmptyExpression();

        public EmptyExpression(SourceSpan span = default(SourceSpan))
        {
            Type = TypeManager.CoreTypes.Object;
            Span = span;
        }

        public EmptyExpression(Type type, SourceSpan span = default(SourceSpan))
        {
            Type = type;
            Span = span;
        }

        internal EmptyExpression()
        {
            // For cloning purposes only.
        }

        public static EmptyExpression Grab()
        {
            var result = _temp ?? new EmptyExpression();
            _temp = null;
            return result;
        }

        public static void Release(EmptyExpression e)
        {
            _temp = e;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            return this;
        }
    }
}