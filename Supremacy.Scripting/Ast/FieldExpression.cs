using System;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   Fully resolved expression that evaluates to a Field.
    /// </summary>
    public class FieldExpression : MemberExpression
    {
        private Type _containerType;
        private FieldInfo _fieldInfo;

        protected FieldExpression(SourceSpan span)
        {
            Span = span;
        }

        internal FieldExpression()
        {
            // For cloning purposes only.
        }

        public FieldExpression(FieldInfo fieldInfo, SourceSpan span)
            : this(null, fieldInfo, span) { }

        public FieldExpression(Type containerType, FieldInfo fieldInfo, SourceSpan span)
        {
            _containerType = containerType;

            FieldInfo = fieldInfo;
            Span = span;
        }

        public FieldExpression(Type containerType, FieldInfo fieldInfo, Type genericType, SourceSpan span)
            : this(containerType, fieldInfo, span)
        {
            if (TypeManager.IsGenericTypeDefinition(genericType))
                return;
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as FieldExpression;
            if (clone == null)
                return;

            clone._containerType = _containerType;
            clone._fieldInfo = _fieldInfo;
        }

        public override string Name
        {
            get { return _fieldInfo.Name; }
        }

        public override bool IsInstance
        {
            get { return !_fieldInfo.IsStatic; }
        }

        public override bool IsStatic
        {
            get { return _fieldInfo.IsStatic; }
        }

        public override Type DeclaringType
        {
            get { return _fieldInfo.DeclaringType; }
        }

        public override string GetSignatureForError()
        {
            return TypeManager.GetFullNameSignature(_fieldInfo);
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return MSAst.Field(
                InstanceExpression.Transform(generator),
                FieldInfo);
        }

        public override MemberExpression ResolveMemberAccess(ParseContext ec, Expression left, SourceSpan loc, NameExpression original)
        {
            var fi = TypeManager.GetGenericFieldDefinition(_fieldInfo);
            Type t = fi.FieldType;

            if (t.IsPointer/* && !ec.IsUnsafe*/)
            {
                // TODO: UnsafeError(ec, loc);
            }

            return base.ResolveMemberAccess(ec, left, loc, original);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            return DoResolve(ec, false, false);
        }

        private Expression DoResolve(ParseContext ec, bool leftValue, bool outAccess)
        {
            if (!_fieldInfo.IsStatic)
            {
                if (InstanceExpression == null)
                {
                    //
                    // This can happen when referencing an instance field using
                    // a fully qualified type expression: TypeName.InstanceField = xxx
                    // 

                    // TODO: SimpleName.Error_ObjectRefRequired(ec, loc, GetSignatureForError());
                    return null;
                }

                // Resolve the field's instance expression while flow analysis is turned
                // off: when accessing a field "a.b", we must check whether the field
                // "a.b" is initialized, not whether the whole struct "a" is initialized.

                if (leftValue)
                {
                    var rightSide = outAccess
                                        ? EmptyExpression.LValueMemberOutAccess
                                        : EmptyExpression.LValueMemberAccess;

                    if (InstanceExpression != EmptyExpression.Null)
                        InstanceExpression = InstanceExpression.ResolveLValue(ec, rightSide);
                }
                else
                {
                    const ResolveFlags rf = ResolveFlags.VariableOrValue | ResolveFlags.DisableFlowAnalysis;

                    if (InstanceExpression != EmptyExpression.Null)
                        InstanceExpression = InstanceExpression.Resolve(ec, rf);
                }

                if (InstanceExpression == null)
                    return null;
            }

            // TODO: the code above uses some non-standard multi-resolve rules
            if (ExpressionClass != ExpressionClass.Invalid)
                return this;

            ExpressionClass = ExpressionClass.Variable;

            // If the instance expression is a local variable or parameter.
                return this;
        }

        static readonly int[] _codes = {
			191,	// instance, write access
			192,	// instance, out access
			198,	// static, write access
			199,	// static, out access
			1648,	// member of value instance, write access
			1649,	// member of value instance, out access
			1650,	// member of value static, write access
			1651	// member of value static, out access
		};

        static readonly string[] _msgs = {
			/*0191*/ "A readonly field '{0}' cannot be assigned to (except in a constructor or a variable initializer).",
			/*0192*/ "A readonly field '{0}' cannot be passed ref or out (except in a constructor).",
			/*0198*/ "A static readonly field '{0}' cannot be assigned to (except in a static constructor or a variable initializer).",
			/*0199*/ "A static readonly field '{0}' cannot be passed ref or out (except in a static constructor).",
			/*1648*/ "Members of readonly field '{0}' cannot be modified (except in a constructor or a variable initializer).",
			/*1649*/ "Members of readonly field '{0}' cannot be passed ref or out (except in a constructor).",
			/*1650*/ "Fields of static readonly field '{0}' cannot be assigned to (except in a static constructor or a variable initializer).",
			/*1651*/ "Fields of static readonly field '{0}' cannot be passed ref or out (except in a static constructor)."
		};

        // The return value is always null.  Returning a value simplifies calling code.
        Expression Report_AssignToReadonly(ParseContext ec, Expression rightSide)
        {
            int i = 0;
            if (rightSide == EmptyExpression.OutAccess || rightSide == EmptyExpression.LValueMemberOutAccess)
                i += 1;
            if (IsStatic)
                i += 2;
            if (rightSide == EmptyExpression.LValueMemberAccess || rightSide == EmptyExpression.LValueMemberOutAccess)
                i += 4;
            ec.ReportError(_codes[i], string.Format(_msgs[i], GetSignatureForError()), Span);

            return null;
        }

        override public Expression DoResolveLValue(ParseContext ec, Expression rightSide)
        {
            var leftValue = !_fieldInfo.IsStatic && TypeManager.IsValueType(_fieldInfo.DeclaringType);
            var outAccess = rightSide == EmptyExpression.OutAccess || rightSide == EmptyExpression.LValueMemberOutAccess;

            var e = DoResolve(ec, leftValue, outAccess);

            if (e == null)
                return null;

            if (_fieldInfo.IsInitOnly)
            {
                // InitOnly fields can only be assigned in constructors or initializers
                //if (!ec.HasAny(ParseContext.Options.FieldInitializerScope | ParseContext.Options.ConstructorScope))
                return Report_AssignToReadonly(ec, rightSide);
            }

            if (rightSide == EmptyExpression.OutAccess &&
                !IsStatic && TypeManager.CoreTypes.MarshalByRefObject != null && TypeManager.IsSubclassOf(DeclaringType, TypeManager.CoreTypes.MarshalByRefObject))
            {
                ec.ReportError(
                    197,
                    string.Format(
                        "Passing '{0}' as ref or out or taking its address may cause a runtime exception because it is a field of a marshal-by-reference class.",
                        GetSignatureForError()),
                    Severity.Warning,
                    Span);
            }

            ExpressionClass = ExpressionClass.Variable;
            return this;
        }

        public override int GetHashCode()
        {
            return _fieldInfo.GetHashCode();
        }

        public FieldInfo FieldInfo
        {
            get { return _fieldInfo; }
            set
            {
                _fieldInfo = value;
                Type = (_fieldInfo == null) ? null : _fieldInfo.FieldType;
            }
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            if (IsStatic)
                sw.Write(TypeManager.GetCSharpName(_containerType));
            else
                DumpChild(InstanceExpression, sw, indentChange);

            sw.Write(".");
            sw.Write(_fieldInfo.Name);
        }

        public override bool Equals(object obj)
        {
            var fe = obj as FieldExpression;
            if (fe == null)
                return false;

            if (_fieldInfo != fe._fieldInfo)
                return false;

            if (InstanceExpression == null || fe.InstanceExpression == null)
                return true;

            return InstanceExpression.Equals(fe.InstanceExpression);
        }
    }
}