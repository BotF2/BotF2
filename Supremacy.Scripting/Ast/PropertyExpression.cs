using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Microsoft.Scripting;

using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   Expression that evaluates to a Property.  The Assign class
    ///   might set the `Value' expression if we are in an assignment.
    ///
    ///   This is not an LValue because we need to re-write the expression, we
    ///   can not take data from the stack and store it.  
    /// </summary>
    public class PropertyExpression : MemberExpression
    {
        private readonly Type _containerType;
        private readonly PropertyInfo _propertyInfo;
        private MethodInfo _getter, _setter;
        private bool _isStatic;

        bool _resolved;
        TypeArguments _typeArguments;

        public PropertyExpression(Type containerType, PropertyInfo propertyInfo, SourceSpan span)
        {
            _isStatic = false;
            _containerType = containerType;
            _propertyInfo = propertyInfo;

            Type = propertyInfo.PropertyType;
            ExpressionClass = ExpressionClass.PropertyAccess;
            Span = span;

            ResolveAccessors(containerType);
        }

        public override string Name
        {
            get { return _propertyInfo.Name; }
        }

        public override bool IsInstance
        {
            get { return !_isStatic; }
        }

        public override bool IsStatic
        {
            get { return _isStatic; }
        }

        public override Type DeclaringType
        {
            get { return _propertyInfo.DeclaringType; }
        }

        public PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return MSAst.Property(
                (InstanceExpression == null) ? null : InstanceExpression.Transform(generator),
                PropertyInfo);
        }

        public override string GetSignatureForError()
        {
            return TypeManager.GetFullNameSignature(_propertyInfo);
        }

        void FindAccessors(Type invocationType)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Static | BindingFlags.Instance |
                                       BindingFlags.DeclaredOnly;

            Type current = _propertyInfo.DeclaringType;
            for (; current != null; current = current.BaseType)
            {
                MemberInfo[] group = TypeManager.MemberLookup(
                    invocationType,
                    invocationType,
                    current,
                    MemberTypes.Property,
                    flags,
                    _propertyInfo.Name,
                    null);

                if (group == null)
                    continue;

                if (group.Length != 1)
                    // Oooops, can this ever happen ?
                    return;

                var pi = (PropertyInfo)group[0];

                if (_getter == null)
                    _getter = pi.GetGetMethod(true);

                if (_setter == null)
                    _setter = pi.GetSetMethod(true);

                MethodInfo accessor = _getter ?? _setter;

                if (!accessor.IsVirtual)
                    return;
            }
        }

        //
        // We also perform the permission checking here, as the PropertyInfo does not
        // hold the information for the accessibility of its setter/getter
        //
        // TODO: Refactor to use some kind of cache together with GetPropertyFromAccessor
        void ResolveAccessors(Type containerType)
        {
            FindAccessors(containerType);

            if (_getter != null)
            {
                var theGetter = TypeManager.DropGenericMethodArguments(_getter);
                _isStatic = theGetter.IsStatic;
            }

            if (_setter != null)
            {
                var theSetter = TypeManager.DropGenericMethodArguments(_setter);
                _isStatic = theSetter.IsStatic;
            }
        }

        bool InstanceResolve(ParseContext ec, bool leftInstance, bool mustDoCs1540Check)
        {
            if (_isStatic)
            {
                InstanceExpression = null;
                return true;
            }

            if (InstanceExpression == null)
            {
                // TODO: SimpleName.Error_ObjectRefRequired(ec, loc, GetSignatureForError());
                return false;
            }

            InstanceExpression = InstanceExpression.DoResolve(ec);
            if (leftInstance && InstanceExpression != null)
                InstanceExpression = InstanceExpression.ResolveLValue(ec, EmptyExpression.LValueMemberAccess);

            if (InstanceExpression == null)
                return false;

            //InstanceExpression.CheckMarshalByRefAccess(ec);

            if (mustDoCs1540Check && (InstanceExpression != EmptyExpression.Null) &&
                !TypeManager.IsInstantiationOfSameGenericType(InstanceExpression.Type, null /*ec.CurrentType*/) &&
                !TypeManager.IsNestedChildOf(null /*ec.CurrentType*/, InstanceExpression.Type)/* &&
                !TypeManager.IsSubclassOf(InstanceExpression.ExpressionType, ec.CurrentType)*/)
            {
                //Error_CannotAccessProtected(ec, loc, _propertyInfo, InstanceExpression.ExpressionType, ec.CurrentType);
                return false;
            }

            return true;
        }

        void Error_PropertyNotFound(ParseContext ec, MethodInfo mi, bool getter)
        {
            // TODO: correctly we should compare arguments but it will lead to bigger changes
            if (mi is MethodBuilder)
            {
                //Error_TypeDoesNotContainDefinition(ec, loc, _propertyInfo.DeclaringType, Name);
                return;
            }

            var sig = new StringBuilder(TypeManager.GetCSharpName(mi.DeclaringType));
            sig.Append('.');
            var iparams = TypeManager.GetParameterData(mi);
            sig.Append(getter ? "get_" : "set_");
            sig.Append(Name);
            sig.Append(iparams.GetSignatureForError());

            ec.ReportError(
                1546,
                string.Format(
                    "Property '{0}' is not supported by the language.  Try to call the accessor method '{1}' directly.",
                    Name,
                    sig),
                Severity.Error,
                Span);
        }

        public bool IsAccessibleFrom(Type invocationType, bool lvalue)
        {
            bool dummy;
            MethodInfo accessor = lvalue ? _setter : _getter;
            if (accessor == null && lvalue)
                accessor = _getter;
            return accessor != null && IsAccessorAccessible(invocationType, accessor, out dummy);
        }

        bool IsSingleDimensionalArrayLength()
        {
            if (DeclaringType != TypeManager.CoreTypes.Array || _getter == null || Name != "Length")
                return false;

            string tName = InstanceExpression.Type.Name;
            int tNameLen = tName.Length;
            return tNameLen > 2 && tName[tNameLen - 2] == '[';
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if (_resolved)
                return this;

            var mustDoCs1540Check = false;
            var result = ResolveGetter(ec, ref mustDoCs1540Check);

            if (!result)
            {
                if (InstanceExpression != null)
                {
                    var exprType = InstanceExpression.Type;
/*
                    var exMethodLookup = ec.LookupExtensionMethod(exprType, Name, loc);
                    if (exMethodLookup != null)
                    {
                        exMethodLookup.ExtensionExpression = InstanceExpression;
                        exMethodLookup.SetTypeArguments(ec, targs);
                        return exMethodLookup.DoResolve(ec);
                    }
*/
                }

                ResolveGetter(ec, ref mustDoCs1540Check);
                return null;
            }

            if (!InstanceResolve(ec, false, mustDoCs1540Check))
                return null;

/*
            //
            // Only base will allow this invocation to happen.
            //
            if (IsBase && _getter.IsAbstract)
            {
                Error_CannotCallAbstractBase(ec, TypeManager.GetFullNameSignature(_propertyInfo));
            }
*/

            if (_propertyInfo.PropertyType.IsPointer)
            {
                // TODO: UnsafeError(ec, loc);
            }

            _resolved = true;

            return this;
        }

        override public Expression DoResolveLValue(ParseContext parseContext, Expression rightSide)
        {
            if (rightSide == EmptyExpression.OutAccess)
            {
                if (parseContext.CurrentScope.TopLevel.GetParameterReference(_propertyInfo.Name, Span) is MemberAccessExpression)
                {
                    parseContext.ReportError(
                        1939,
                        string.Format(
                            "A range variable '{0}' may not be passed as a 'ref' or 'out' parameter.",
                            _propertyInfo.Name),
                        Severity.Error,
                        Span);
                }
                else
                {
                    parseContext.ReportError(
                        206,
                        string.Format(
                            "A property or indexer '{0}' may not be passed as a 'ref' or 'out' parameter.",
                            _propertyInfo.Name),
                        Severity.Error,
                        Span);
                }
                return null;
            }

            if (rightSide == EmptyExpression.LValueMemberAccess || rightSide == EmptyExpression.LValueMemberOutAccess)
            {
                // TODO: Error_CannotModifyIntermediateExpressionValue(parseContext);
            }

            if (_setter == null)
            {
                //
                // The following condition happens if the PropertyExpr was
                // created, but is invalid (ie, the property is inaccessible),
                // and we did not want to embed the knowledge about this in
                // the caller routine.  This only avoids double error reporting.
                //
                if (_getter == null)
                    return null;

                if (parseContext.CurrentScope.TopLevel.GetParameterReference(_propertyInfo.Name, Span) is MemberAccessExpression)
                {
                    parseContext.ReportError(
                        1947,
                        string.Format(
                            "A range variable '{0}' cannot be assigned to.  Consider using 'let' clause to store the value.",
                            _propertyInfo.Name),
                        Span);
                }
                else
                {
                    parseContext.ReportError(
                        200,
                        string.Format(
                            "Property or indexer '{0}' cannot be assigned to (it is read only).",
                            GetSignatureForError()),
                        Span);
                }

                return null;
            }

            if ((_typeArguments != null) && (_typeArguments.Count != 0))
            {
                base.SetTypeArguments(parseContext, _typeArguments);
                return null;
            }

            if (TypeManager.GetParameterData(_setter).Count != 1)
            {
                Error_PropertyNotFound(parseContext, _setter, false);
                return null;
            }

            bool mustDoCs1540Check;
            if (!IsAccessorAccessible(null /*parseContext.CurrentType*/, _setter, out mustDoCs1540Check))
            {
/*
                PropertyBase.PropertyMethod pm = TypeManager.GetMethod(_setter) as PropertyBase.PropertyMethod;
                if (pm != null && pm.HasCustomAccessModifier)
                {
                    parseContext.Report.SymbolRelatedToPreviousError(pm);
                    parseContext.Report.Error(272, this.Span, "The property or indexer `{0}' cannot be used in this context because the set accessor is inaccessible",
                        TypeManager.CSharpSignature(_setter));
                }
                else
*/
                {
                    parseContext.ReportError(
                        CompilerErrors.MemberIsInaccessible,
                        Span,
                        TypeManager.GetCSharpSignature(_setter));
                }

                return null;
            }

            if (!InstanceResolve(parseContext, TypeManager.IsStruct(_propertyInfo.DeclaringType), mustDoCs1540Check))
                return null;

/*
            //
            // Only base will allow this invocation to happen.
            //
            if (IsBase && _setter.IsAbstract)
            {
                Error_CannotCallAbstractBase(parseContext, TypeManager.GetFullNameSignature(_propertyInfo));
            }
*/

            if (_propertyInfo.PropertyType.IsPointer/* && !parseContext.IsUnsafe*/)
            {
                // TODO: UnsafeError(parseContext, this.Span);
            }

/*
            if (!parseContext.IsObsolete)
            {
                PropertyBase pb = TypeManager.GetProperty(_propertyInfo);
                if (pb != null)
                {
                    pb.CheckObsoleteness(this.Span);
                }
                else
                {
                    ObsoleteAttribute oa = AttributeTester.GetMemberObsoleteAttribute(_propertyInfo);
                    if (oa != null)
                        AttributeTester.Report_ObsoleteMessage(oa, GetSignatureForError(), this.Span, parseContext.Report);
                }
            }
*/

            return this;
        }

        bool ResolveGetter(ParseContext ec, ref bool mustDoCs1540Check)
        {
            if ((_typeArguments != null) && _typeArguments.Any())
            {
                base.SetTypeArguments(ec, _typeArguments);
                return false;
            }

            if (_getter != null)
            {
                if (TypeManager.GetParameterData(_getter).Count != 0)
                {
                    Error_PropertyNotFound(ec, _getter, true);
                    return false;
                }
            }

            if (_getter == null)
            {
                //
                // The following condition happens if the PropertyExpr was
                // created, but is invalid (ie, the property is inaccessible),
                // and we did not want to embed the knowledge about this in
                // the caller routine.  This only avoids double error reporting.
                //
                if (_setter == null)
                    return false;

                if (InstanceExpression != EmptyExpression.Null)
                {
                    ec.ReportError(
                        154,
                        string.Format(
                            "The property or indexer '{0}' cannot be used in this context because it lacks the 'get' accessor.",
                            TypeManager.GetFullNameSignature(_propertyInfo)),
                        Span);

                    return false;
                }
            }

            if (_getter != null &&
                !IsAccessorAccessible(null /*ec.CurrentType*/, _getter, out mustDoCs1540Check))
            {
/*
                PropertyBase.PropertyMethod pm = TypeManager.GetMethod(_getter) as PropertyBase.PropertyMethod;
                if (pm != null && pm.HasCustomAccessModifier)
                {
                    ec.Report.SymbolRelatedToPreviousError(pm);
                    ec.Report.Error(271, this.Span, "The property or indexer `{0}' cannot be used in this context because the get accessor is inaccessible",
                        TypeManager.CSharpSignature(_getter));
                }
                else
*/
                {
                    ec.ReportError(
                        CompilerErrors.MemberIsInaccessible,
                        Span,
                        TypeManager.GetCSharpSignature(_getter));
                }

                return false;
            }

            return true;
        }

        public override void SetTypeArguments(ParseContext ec, TypeArguments ta)
        {
            _typeArguments = ta;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            if (_isStatic)
                sw.Write(TypeManager.GetCSharpName(_containerType));
            else
                DumpChild(InstanceExpression, sw, indentChange);

            sw.Write(".");
            sw.Write(_propertyInfo.Name);
        }
    }

}