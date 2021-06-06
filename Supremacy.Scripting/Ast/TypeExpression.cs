using System;

using Microsoft.Scripting;

using Supremacy.Annotations;

using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class TypeExpression : FullNamedExpression
    {
        public override bool IsPrimaryExpression => true;

        protected TypeExpression() { }

        public TypeExpression(Type type)
        {
            Type = type ?? throw new ArgumentNullException("type");
        }

        public TypeExpression(Type type, SourceSpan span)
            : this(type)
        {
            Span = span;
        }

        public static TypeExpression Create([NotNull] Type type, SourceSpan span = default(SourceSpan))
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return type.IsGenericType && !type.IsGenericTypeDefinition
                ? new GenericTypeExpression(
                    type,
                    new TypeArguments(type.GetGenericArguments().Select(o => Create(o, span)).ToArray()),
                    span)
                : new TypeExpression(type, span);
        }

        public override System.Linq.Expressions.Expression TransformCore(ScriptGenerator generator)
        {
            return System.Linq.Expressions.Expression.Constant(
                Type,
                typeof(Type));
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(TypeManager.GetCSharpName(Type));
        }

        public override FullNamedExpression ResolveAsTypeStep(ParseContext ec, bool silent)
        {
            TypeExpression resolvedType = DoResolveAsTypeStep(ec);
            if (resolvedType == null)
            {
                return null;
            }

            ExpressionClass = ExpressionClass.Type;
            return resolvedType;
        }

        protected virtual TypeExpression DoResolveAsTypeStep(ParseContext parseContext)
        {
            return this;
        }

        public virtual bool CheckAccessLevel(ParseContext parseContext)
        {
            return TypeManager.CheckAccessLevel(parseContext, Type);
        }


        public virtual bool IsClass => Type.IsClass;

        public virtual bool IsValueType => TypeManager.IsStruct(Type);

        public virtual bool IsInterface => Type.IsInterface;

        public virtual bool IsSealed => Type.IsSealed;

        public virtual bool CanInheritFrom()
        {
            if (Type == TypeManager.CoreTypes.Enum ||
                Type == TypeManager.CoreTypes.ValueType ||
                Type == TypeManager.CoreTypes.MulticastDelegate ||
                Type == TypeManager.CoreTypes.Delegate ||
                Type == TypeManager.CoreTypes.Array)
            {
                return false;
            }
            return true;
        }
    }
}