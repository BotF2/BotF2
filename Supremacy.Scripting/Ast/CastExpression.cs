using System;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class CastExpression : Expression
    {
        private Expression _child;
        private FullNamedExpression _destinationType;

        public CastExpression([NotNull] Expression child, [NotNull] Type destinationType)
        {
            if (child == null)
                throw new ArgumentNullException("child");
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            _child = child;
            _destinationType = new TypeExpression(destinationType);

            Type = destinationType;
        }

        public CastExpression([NotNull] Expression child, [NotNull] FullNamedExpression destinationType)
        {
            if (child == null)
                throw new ArgumentNullException("child");
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            _child = child;
            _destinationType = destinationType;

            Type = destinationType.Type;
        }

        public CastExpression() { }

        public Type UnderlyingType
        {
            get { return (Child == null) ? null : Child.Type; }
        }

        public FullNamedExpression DestinationType
        {
            get { return _destinationType; }
            set
            {
                _destinationType = value;
                Type = _destinationType.Type;
            }
        }

        public Expression Child
        {
            get { return _child; }
            set { _child = value; }
        }

        public override bool IsNull
        {
            get { return Child.IsNull; }
        }

        public override bool ShouldParenthesize
        {
            get { return true; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _child, prefix, postfix);
            Walk(ref _destinationType, prefix, postfix);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            if (ExpressionClass != ExpressionClass.Invalid)
                return this;

            _destinationType = _destinationType.ResolveAsTypeStep(parseContext, false);

            Type = _destinationType.Type;

            if (Type == null)
                return null;

            var childLambda = _child as LambdaExpression;
            if (childLambda != null)
                childLambda.ImplicitStandardConversionExists(parseContext, Type);

            _child = _child.Resolve(parseContext);

            ExpressionClass = ExpressionClass.Value;

            return this;
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return generator.ConvertTo(
                _destinationType.Type,
                ConversionResultKind.ExplicitCast,
                _child.Transform(generator));
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as CastExpression;
            if (clone == null)
                return;

            clone._child = Clone(cloneContext, _child);
            clone._destinationType = Clone(cloneContext, _destinationType);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("(");
            
            DestinationType.Dump(sw);
            
            sw.Write(") ");

            if (!Child.IsPrimaryExpression)
                sw.Write("(");

            Child.Dump(sw, indentChange);

            if (!Child.IsPrimaryExpression)
                sw.Write(")");
        }
    }

    public class Wrap : CastExpression
    {
        protected Wrap(Expression operand, Type type)
            : base(operand, type)
        {
            ExpressionClass = ExpressionClass.Value;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            var childCast = Child as CastExpression;
            if (childCast != null)
            {
                DestinationType = DestinationType;
                return childCast.DoResolve(ec);
            }

            return base.DoResolve(ec);
        }

        public static Expression Create(Expression child, Type type)
        {
            // Avoid unwraping and wraping of the same type
            var unwrap = child as Unwrap;
            if (unwrap != null && TypeManager.IsEqual(child.Type, type.GetGenericArguments()[0]))
                return unwrap.Original;

            return new Wrap(child, type);
        }
    }

    public class Unwrap : Expression
    {
        private Expression _child;
        private NullableInfo _info;

        private Unwrap(Expression child)
        {
            _child = child;
            _info = new NullableInfo(child.Type);

            Span = child.Span;
            Type = _info.UnderlyingType;
            ExpressionClass = child.ExpressionClass;
        }

        internal Unwrap()
        {
            // Used for cloning only.
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as Unwrap;
            if (clone == null)
                return;

            clone._child = Clone(cloneContext, _child);
            clone._info = _info;
        }

        public Expression Original
        {
            get { return _child; }
        }

        public override bool IsNull
        {
            get { return _child.IsNull; }
        }

        public static Expression Create(Expression expr)
        {
            // Avoid unwraping and wraping of same type
            var wrap = expr as Wrap;
            if (wrap != null)
                return wrap.Child;

            return Create(expr, false);
        }

        public static Unwrap Create(Expression expr, bool useDefaultValue)
        {
            return new Unwrap(expr);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            return this;
        }

        public override Expression DoResolveLValue(ParseContext ec, Expression rightSide)
        {
            return DoResolve(ec);
        }

        public override bool Equals(object obj)
        {
            var unwrap = obj as Unwrap;
            return (unwrap != null) && _child.Equals(unwrap._child);
        }

        public override int GetHashCode()
        {
            return _child.GetHashCode();
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return _child.Transform(generator);
        }

        #region Nested type: InternalWrap
        protected class InternalWrap : Expression
        {
            private Expression _child;
            private NullableInfo _info;

            public InternalWrap([NotNull] Expression child, [NotNull] NullableInfo info, SourceSpan location)
            {
                if (child == null)
                    throw new ArgumentNullException("child");
                if (info == null)
                    throw new ArgumentNullException("info");

                _child = child;
                _info = info;

                Span = location;
                Type = _info.Type;
                ExpressionClass = ExpressionClass.Value;
            }

            public override void CloneTo<T>(CloneContext cloneContext, T target)
            {
                base.CloneTo(cloneContext, target);

                var clone = target as InternalWrap;
                if (clone == null)
                    return;

                clone._child = Clone(cloneContext, _child);
                clone._info = _info;
            }

            public Expression Child
            {
                get { return _child; }
            }

            public override Expression DoResolve(ParseContext ec)
            {
                return this;
            }
        }
        #endregion
    }

    public sealed class NullableInfo
    {
        public readonly Type Type;
        public readonly Type UnderlyingType;
        public ConstructorInfo Constructor;
        public MethodInfo GetValueOrDefault;
        public MethodInfo HasValue;
        public MethodInfo Value;

        public NullableInfo(Type type)
        {
            Type = type;
            UnderlyingType = type.GetGenericArguments()[0];

            var hasValuePi = type.GetProperty("HasValue", Type.EmptyTypes);
            var valuePi = type.GetProperty("Value", Type.EmptyTypes);

            GetValueOrDefault = type.GetMethod("GetValueOrDefault", Type.EmptyTypes);
            HasValue = hasValuePi.GetGetMethod(false);
            Value = valuePi.GetGetMethod(false);

            Constructor = type.GetConstructor(new[] { UnderlyingType });
        }
    }

    public class EmptyCastExpression : CastExpression
    {
        private EmptyCastExpression(Expression child, Type targetType)
            : base(child, targetType)
        {
        }

        public static Expression Create(Expression child, Type type)
        {
            var c = child as ConstantExpression;
            if (c != null)
                return new EmptyConstantCastExpression(c, type);

            var e = child as EmptyCastExpression;
            if (e != null)
                return new EmptyCastExpression(e.Child, type);

            return new EmptyCastExpression(child, type);
        }
    }

    /// <summary>
    ///   This kind of cast is used to encapsulate Value Types in objects.
    ///
    ///   The effect of it is to box the value type emitted by the previous
    ///   operation.
    /// </summary>
    public class BoxedCastExpression : CastExpression
    {

        public BoxedCastExpression(Expression expr, Type targetType)
            : base(expr, targetType)
        {
            ExpressionClass = ExpressionClass.Value;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            // This should never be invoked, we are born in fully
            // initialized state.

            return this;
        }
    }
}