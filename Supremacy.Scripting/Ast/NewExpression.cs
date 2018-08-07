using System;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Annotations;

using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class NewExpression : Expression
    {
        private Expression _requestedType;
        private Arguments _arguments;

        private Type _resolvedType;
        private MethodGroupExpression _constructor;

        public NewExpression ([NotNull] Expression requestedType, Arguments arguments, SourceSpan location)
        {
            if (requestedType == null)
                throw new ArgumentNullException("requestedType");

            _requestedType = requestedType;
            _arguments = arguments;

            Span = location;
        }

        public MethodGroupExpression Constructor
        {
            get { return _constructor; }
        }

        public Arguments Arguments
        {
            get { return _arguments; }
        }

        protected virtual bool HasInitializer
        {
            get { return false; }
        }

        public bool IsDefaultValueType
        {
            get
            {
                return TypeManager.IsValueType(Type) &&
                       !HasInitializer &&
                       ((Arguments == null) || (Arguments.Count == 0));
            }
        }

        public static ConstantExpression Constantify (Type t)
		{
			if (t == TypeManager.CoreTypes.Int32)
				return new ConstantExpression<int>(0);
			if (t == TypeManager.CoreTypes.UInt32)
                return new ConstantExpression<uint>(0u);
			if (t == TypeManager.CoreTypes.Int64)
                return new ConstantExpression<long>(0L);
			if (t == TypeManager.CoreTypes.UInt64)
                return new ConstantExpression<ulong>(0uL);
			if (t == TypeManager.CoreTypes.Single)
                return new ConstantExpression<float>(0f);
			if (t == TypeManager.CoreTypes.Double)
                return new ConstantExpression<double>(0d);
			if (t == TypeManager.CoreTypes.Int16)
                return new ConstantExpression<short>(0);
			if (t == TypeManager.CoreTypes.UInt16)
                return new ConstantExpression<ushort>(0);
			if (t == TypeManager.CoreTypes.SByte)
                return new ConstantExpression<sbyte>(0);
			if (t == TypeManager.CoreTypes.Byte)
                return new ConstantExpression<byte>(0);
			if (t == TypeManager.CoreTypes.Char)
                return new ConstantExpression<char>('\0');
			if (t == TypeManager.CoreTypes.Boolean)
                return new ConstantExpression<bool>(false);
			if (t == TypeManager.CoreTypes.Decimal)
                return new ConstantExpression<decimal>(0m);
            if (TypeManager.IsEnumType(t))
                return new EnumConstantExpression(Constantify(Enum.GetUnderlyingType(t)), t);
			if (TypeManager.IsNullableType (t))
				return LiftedNullExpression.Create(t, SourceSpan.None);

			return null;
		}

        public override Expression DoResolve(ParseContext ec)
        {
            var typeExpression = _requestedType.ResolveAsTypeTerminal(ec, false);
            if (typeExpression == null)
                return null;

            Type = _resolvedType = typeExpression.Type;

            if (Arguments == null)
            {
                var c = Constantify(_resolvedType);
                if (c != null)
                    return ReducedExpression.Create(c, this);
            }

            if (TypeManager.IsGenericParameter(_resolvedType))
            {
                var gc = TypeManager.GetTypeParameterConstraints(_resolvedType);

                if ((gc == null) || (!gc.HasConstructorConstraint && !gc.IsValueType))
                {
                    ec.ReportError(
                        304,
                        string.Format(
                            "Cannot create an instance of the variable type '{0}' because it doesn't have the new() constraint.",
                            TypeManager.GetCSharpName(_resolvedType)),
                        Span);

                    return null;
                }

                if ((_arguments != null) && (_arguments.Count != 0))
                {
                    ec.ReportError(
                        417,
                        string.Format(
                            "'{0}': cannot provide arguments when creating an instance of a variable type.",
                            TypeManager.GetCSharpName(_resolvedType)),
                        Span);

                    return null;
                }

                ExpressionClass = ExpressionClass.Value;
                return this;
            }

            if (_resolvedType.IsAbstract && _resolvedType.IsSealed)
            {
                ec.ReportError(
                    712,
                    string.Format(
                        "Cannot create an instance of the static class '{0}'.",
                        TypeManager.GetCSharpName(_resolvedType)),
                    Span);

                return null;
            }

            if (_resolvedType.IsInterface || _resolvedType.IsAbstract)
            {
                ec.ReportError(
                    144,
                    string.Format(
                        "Cannot create an instance of the abstract class or interface '{0}'.",
                        TypeManager.GetCSharpName(_resolvedType)),
                    Span);

                return null;
            }

            var isStruct = TypeManager.IsStruct(_resolvedType);
            ExpressionClass = ExpressionClass.Value;

            //
            // SRE returns a match for .ctor () on structs (the object constructor), 
            // so we have to manually ignore it.
            //
            if (isStruct && (_arguments != null) && !_arguments.Any())
                return this;

            // For member-lookup, treat 'new Foo (bar)' as call to 'foo.ctor (bar)', where 'foo' is of type 'Foo'.
            var memberLookup = MemberLookupFinal(
                ec,
                _resolvedType,
                _resolvedType,
                ConstructorInfo.ConstructorName,
                MemberTypes.Constructor,
                AllBindingFlags | BindingFlags.DeclaredOnly,
                Span);

            if (_arguments != null)
                _arguments.Resolve(ec);

            if (memberLookup == null)
                return null;

            _constructor = memberLookup as MethodGroupExpression;
            
            if (_constructor == null)
            {
                memberLookup.OnErrorUnexpectedKind(ec, ResolveFlags.MethodGroup, Span);
                return null;
            }

            _constructor = _constructor.OverloadResolve(ec, ref _arguments, false, Span);

            return (_constructor == null) ? null : this;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _requestedType, prefix, postfix);

            if (_arguments != null)
                WalkList(_arguments, prefix, postfix);
        }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            var constructorInfo = (ConstructorInfo)_constructor;
            //if (this.ExpressionType.IsGenericType)
            return MSAst.Expression.New(
                constructorInfo,
                Arguments.Transform(_arguments, generator));
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("new");

            if (_resolvedType == null)
            {
                sw.Write(" ");
                DumpChild(_requestedType, sw, indentChange);
            }
            else if (!TypeManager.IsAnonymousType(_resolvedType))
            {
                sw.Write(" ");
                sw.Write(TypeManager.GetCSharpName(_resolvedType));
            }

            var hasArguments = (_arguments != null) && _arguments.Any();

            if (hasArguments || !HasInitializer)
                sw.Write("(");

            if (hasArguments)
            {
                for (int i = 0; i < _arguments.Count; i++)
                {
                    if (i == 0)
                        sw.Write(", ");
                    DumpChild(_arguments[i], sw, indentChange);
                }
            }

            if (hasArguments || !HasInitializer)
                sw.Write(")");
        }
    }
}