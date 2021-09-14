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

        public NewExpression([NotNull] Expression requestedType, Arguments arguments, SourceSpan location)
        {
            _requestedType = requestedType ?? throw new ArgumentNullException("requestedType");
            _arguments = arguments;

            Span = location;
        }

        public MethodGroupExpression Constructor { get; private set; }

        public Arguments Arguments => _arguments;

        protected virtual bool HasInitializer => false;

        public bool IsDefaultValueType => TypeManager.IsValueType(Type) &&
                       !HasInitializer &&
                       ((Arguments == null) || (Arguments.Count == 0));

        public static ConstantExpression Constantify(Type t)
        {
            if (t == TypeManager.CoreTypes.Int32)
            {
                return new ConstantExpression<int>(0);
            }

            if (t == TypeManager.CoreTypes.UInt32)
            {
                return new ConstantExpression<uint>(0u);
            }

            if (t == TypeManager.CoreTypes.Int64)
            {
                return new ConstantExpression<long>(0L);
            }

            if (t == TypeManager.CoreTypes.UInt64)
            {
                return new ConstantExpression<ulong>(0uL);
            }

            if (t == TypeManager.CoreTypes.Single)
            {
                return new ConstantExpression<float>(0f);
            }

            if (t == TypeManager.CoreTypes.Double)
            {
                return new ConstantExpression<double>(0d);
            }

            if (t == TypeManager.CoreTypes.Int16)
            {
                return new ConstantExpression<short>(0);
            }

            if (t == TypeManager.CoreTypes.UInt16)
            {
                return new ConstantExpression<ushort>(0);
            }

            if (t == TypeManager.CoreTypes.SByte)
            {
                return new ConstantExpression<sbyte>(0);
            }

            if (t == TypeManager.CoreTypes.Byte)
            {
                return new ConstantExpression<byte>(0);
            }

            if (t == TypeManager.CoreTypes.Char)
            {
                return new ConstantExpression<char>('\0');
            }

            if (t == TypeManager.CoreTypes.Boolean)
            {
                return new ConstantExpression<bool>(false);
            }

            if (t == TypeManager.CoreTypes.Decimal)
            {
                return new ConstantExpression<decimal>(0m);
            }

            if (TypeManager.IsEnumType(t))
            {
                return new EnumConstantExpression(Constantify(Enum.GetUnderlyingType(t)), t);
            }

            return TypeManager.IsNullableType(t) ? LiftedNullExpression.Create(t, SourceSpan.None) : null;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            TypeExpression typeExpression = _requestedType.ResolveAsTypeTerminal(ec, false);
            if (typeExpression == null)
            {
                return null;
            }

            Type = _resolvedType = typeExpression.Type;

            if (Arguments == null)
            {
                ConstantExpression c = Constantify(_resolvedType);
                if (c != null)
                {
                    return ReducedExpression.Create(c, this);
                }
            }

            if (TypeManager.IsGenericParameter(_resolvedType))
            {
                GenericConstraints gc = TypeManager.GetTypeParameterConstraints(_resolvedType);

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

            bool isStruct = TypeManager.IsStruct(_resolvedType);
            ExpressionClass = ExpressionClass.Value;

            //
            // SRE returns a match for .ctor () on structs (the object constructor), 
            // so we have to manually ignore it.
            //
            if (isStruct && (_arguments != null) && !_arguments.Any())
            {
                return this;
            }

            // For member-lookup, treat 'new Foo (bar)' as call to 'foo.ctor (bar)', where 'foo' is of type 'Foo'.
            Expression memberLookup = MemberLookupFinal(
                ec,
                _resolvedType,
                _resolvedType,
                ConstructorInfo.ConstructorName,
                MemberTypes.Constructor,
                AllBindingFlags | BindingFlags.DeclaredOnly,
                Span);

            if (_arguments != null)
            {
                _arguments.Resolve(ec);
            }

            if (memberLookup == null)
            {
                return null;
            }

            Constructor = memberLookup as MethodGroupExpression;

            if (Constructor == null)
            {
                memberLookup.OnErrorUnexpectedKind(ec, ResolveFlags.MethodGroup, Span);
                return null;
            }

            Constructor = Constructor.OverloadResolve(ec, ref _arguments, false, Span);

            return (Constructor == null) ? null : this;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _requestedType, prefix, postfix);

            if (_arguments != null)
            {
                WalkList(_arguments, prefix, postfix);
            }
        }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            ConstructorInfo constructorInfo = (ConstructorInfo)Constructor;
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

            bool hasArguments = (_arguments != null) && _arguments.Any();

            if (hasArguments || !HasInitializer)
            {
                sw.Write("(");
            }

            if (hasArguments)
            {
                for (int i = 0; i < _arguments.Count; i++)
                {
                    if (i == 0)
                    {
                        sw.Write(", ");
                    }

                    DumpChild(_arguments[i], sw, indentChange);
                }
            }

            if (hasArguments || !HasInitializer)
            {
                sw.Write(")");
            }
        }
    }
}