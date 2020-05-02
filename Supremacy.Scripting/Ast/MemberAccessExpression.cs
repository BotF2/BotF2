using System;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class MemberAccessExpression : TypeNameExpression
    {
        private NameExpression _memberName;
        private Expression _left;

        public Expression Left
        {
            get => _left;
            set => _left = value;
        }

        public override bool IsPrimaryExpression => true;

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _left, prefix, postfix);
            Walk(ref _memberName, prefix, postfix);
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return generator.GetMember(
                Left.Transform(generator),
                Name);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            bool parenthesize = !_left.IsPrimaryExpression;

            if (parenthesize)
            {
                sw.Write("(");
            }

            _left.Dump(sw, indentChange);

            if (parenthesize)
            {
                sw.Write(")");
            }

            sw.Write(".");
            sw.Write(Name);

            if (HasTypeArguments)
            {
                sw.Write("<");
                for (int i = 0; i < TypeArguments.Count; i++)
                {
                    FullNamedExpression type = TypeArguments[i];
                    if (i != 0)
                    {
                        sw.Write(",");
                    }

                    DumpChild(type, sw, indentChange);
                }
                sw.Write(">");
            }
        }

        public override Expression DoResolve(ParseContext ec)
        {
            return DoResolve(ec, null);
        }

        public override Expression DoResolveLValue(ParseContext parseContext, Expression rightSide)
        {
            return DoResolve(parseContext, rightSide);
        }

        private Expression _resolved;

        public MemberAccessExpression() { }

        public MemberAccessExpression(Expression left, string name, SourceSpan span)
        {
            Left = left;
            Name = name;
            Span = span;
        }

        public MemberAccessExpression(Expression left, string name, TypeArguments typeArguments, SourceSpan span)
            : this(left, name, span)
        {
            TypeArguments.Add(typeArguments);
        }

        private Expression DoResolve(ParseContext ec, Expression rightSide)
        {
            if (_resolved != null)
            {
                return _resolved;
            }

            Type type = Type;
            if (type != null)
            {
                throw new Exception();
            }

            //
            // Resolve the expression with flow analysis turned off, we'll do the definite
            // assignment checks later.  This is because we don't know yet what the expression
            // will resolve to - it may resolve to a FieldExpr and in this case we must do the
            // definite assignment check on the actual field and not on the whole struct.
            //

            NameExpression original = Left as NameExpression;

            Expression leftResolved = Left.Resolve(ec,
                ResolveFlags.VariableOrValue | ResolveFlags.Type |
                ResolveFlags.Intermediate | ResolveFlags.DisableStructFlowAnalysis);

            if (leftResolved == null)
            {
                return null;
            }

            TypeArguments typeArguments = TypeArguments;
            string lookupIdentifier = MemberName.MakeName(Name, typeArguments);

            if (leftResolved is NamespaceExpression ns)
            {
                FullNamedExpression resolved = null;

                MemberTracker namespaceChild;

                bool found = ns.Tracker is NamespaceGroupTracker trackerGroup
                    ? trackerGroup.TryGetValue((SymbolId)lookupIdentifier, out namespaceChild)
                    : ns.Tracker.TryGetValue((SymbolId)lookupIdentifier, out namespaceChild);
                if (!found)
                {
                    // TODO: ns.Error_NamespaceDoesNotExist(loc, lookupIdentifier, ec.Report);
                }
                else if (HasTypeArguments)
                {
                    if (!typeArguments.Resolve(ec))
                    {
                        return null;
                    }

                    if (namespaceChild is TypeGroup typeTracker)
                    {
                        TypeTracker matchingType = typeTracker.GetTypeForArity(typeArguments.Count);
                        if (matchingType != null)
                        {
                            resolved = new GenericTypeExpression(
                                matchingType.Type,
                                typeArguments,
                                Span).ResolveAsTypeStep(ec, false);
                        }
                        else
                        {
                            // TODO: Error?
                        }
                    }
                }
                else
                {
                    if (namespaceChild is TypeGroup typeGroup)
                    {
                        if (typeGroup.TryGetNonGenericType(out Type nonGenericType))
                        {
                            resolved = new TypeExpression(nonGenericType) { Span = Span };
                        }
                        else
                        {
                            // TODO: Error?
                        }
                    }
                    else
                    {
                        if (namespaceChild is NestedTypeTracker nestedType)
                        {
                            resolved = new TypeExpression(nestedType.Type) { Span = Span };
                        }
                        else
                        {
                            // TODO: Error?
                        }
                    }
                }

                //FullNamedExpression retval = ns.Lookup(ec, lookupIdentifier, loc);

                if (resolved == null)
                {

                }
                else if (HasTypeArguments)
                {
                    resolved = new GenericTypeExpression(
                        resolved.Type,
                        typeArguments,
                        Span).ResolveAsTypeStep(ec, false);
                }

                _resolved = resolved;
                if (resolved != null)
                {
                    ExpressionClass = resolved.ExpressionClass;
                }

                return resolved;
            }

            Type expressionType = leftResolved.Type;

            if (expressionType.IsPointer || expressionType == TypeManager.CoreTypes.Void)
            {
                // TODO: Unary.Error_OperatorCannotBeApplied(ec, loc, ".", expressionType);
                return null;
            }

            if (leftResolved is ConstantExpression c && c.Value == null)
            {
                ec.ReportError(
                    1720,
                    "Expression will always cause a 'System.NullReferenceException'.",
                    Severity.Warning,
                    Span);
            }

            if (typeArguments != null && !typeArguments.Resolve(ec))
            {
                return null;
            }

            Expression memberLookup = MemberLookup(
                ec,
                null,
                expressionType,
                expressionType,
                Name,
                Span);

            if (memberLookup == null && typeArguments != null)
            {
                memberLookup = MemberLookup(
                    ec,
                    null,
                    expressionType,
                    expressionType,
                    lookupIdentifier,
                    Span);
            }

            if (memberLookup == null)
            {
                ExpressionClass expressionClass = leftResolved.ExpressionClass;

                //
                // Extension methods are not allowed on all expression types
                //
                if (expressionClass == ExpressionClass.Value || expressionClass == ExpressionClass.Variable ||
                    expressionClass == ExpressionClass.IndexerAccess || expressionClass == ExpressionClass.PropertyAccess ||
                    expressionClass == ExpressionClass.EventAccess)
                {
                    ExtensionMethodGroupExpression exMethodLookup = ec.LookupExtensionMethod(expressionType, Name, Span);
                    if (exMethodLookup != null)
                    {
                        exMethodLookup.ExtensionExpression = leftResolved;

                        if ((typeArguments != null) && typeArguments.Any())
                        {
                            exMethodLookup.SetTypeArguments(ec, typeArguments);
                        }

                        _resolved = exMethodLookup.DoResolve(ec);
                        if (_resolved != null)
                        {
                            ExpressionClass = _resolved.ExpressionClass;
                        }

                        return _resolved;
                    }
                }

                Left = leftResolved;
                
                memberLookup = OnErrorMemberLookupFailed(
                    ec,
                    null,
                    expressionType,
                    expressionType,
                    Name,
                    null,
                    AllMemberTypes,
                    AllBindingFlags);

                if (memberLookup == null)
                {
                    return null;
                }
            }

            if (memberLookup is TypeExpression texpr)
            {
                if (!(leftResolved is TypeExpression) &&
                    (original == null || !original.IdenticalNameAndTypeName(ec, leftResolved, Span)))
                {
                    ec.ReportError(
                        572,
                        string.Format(
                            "'{0}': cannot reference a type through an expression; try '{1}' instead.",
                            Name,
                            memberLookup.GetSignatureForError()),
                        Severity.Error,
                        Span);

                    return null;
                }

                if (!texpr.CheckAccessLevel(ec))
                {
                    ec.ReportError(
                        CompilerErrors.MemberIsInaccessible,
                        Span,
                        TypeManager.GetCSharpName(memberLookup.Type));

                    return null;
                }

                if (leftResolved is GenericTypeExpression ct)
                {
                    //
                    // When looking up a nested type in a generic instance
                    // via reflection, we always get a generic type definition
                    // and not a generic instance - so we have to do this here.
                    //
                    // See gtest-172-lib.cs and gtest-172.cs for an example.
                    //

                    TypeArguments nestedTargs;
                    if (HasTypeArguments)
                    {
                        nestedTargs = ct.TypeArguments.Clone();
                        nestedTargs.Add(typeArguments);
                    }
                    else
                    {
                        nestedTargs = ct.TypeArguments;
                    }

                    ct = new GenericTypeExpression(memberLookup.Type, nestedTargs, Span);

                    _resolved = ct.ResolveAsTypeStep(ec, false);
                    if (_resolved != null)
                    {
                        ExpressionClass = _resolved.ExpressionClass;
                    }

                    return _resolved;
                }

                _resolved = memberLookup;
                ExpressionClass = _resolved.ExpressionClass;
                return memberLookup;
            }

            MemberExpression me = (MemberExpression)memberLookup;
            me = me.ResolveMemberAccess(ec, leftResolved, Span, original);
            if (me == null)
            {
                return null;
            }

            if ((typeArguments != null) && typeArguments.Any())
            {
                me.SetTypeArguments(ec, typeArguments);
            }

            if (original != null && !TypeManager.IsValueType(expressionType))
            {
                if (me.IsInstance)
                {

                }
            }

            // The following DoResolve/DoResolveLValue will do the definite assignment
            // check.

            Left = leftResolved;

            return rightSide != null ? (_resolved = me.DoResolveLValue(ec, rightSide)) : (_resolved = me.DoResolve(ec));
        }

        public override FullNamedExpression ResolveAsTypeStep(ParseContext ec, bool silent)
        {
            FullNamedExpression result = ResolveNamespaceOrType(ec, silent);
            Type = result.Type;
            return result;
        }

        public FullNamedExpression ResolveNamespaceOrType(ParseContext rc, bool silent)
        {
            FullNamedExpression resolved = Left.ResolveAsTypeStep(rc, silent);

            if (resolved == null)
            {
                return null;
            }

            ScriptLanguageContext languageContext = (ScriptLanguageContext)rc.Compiler.SourceUnit.LanguageContext;

            bool hasTypeArguments = HasTypeArguments;
            TypeArguments typeArguments = TypeArguments;

            string lookupIdentifier = ReflectionUtils.GetNormalizedTypeName(MemberName.MakeName(Name, typeArguments));


            if (resolved is NamespaceExpression ns)
            {
                MemberTracker tracker;

                bool found = ns.Tracker is NamespaceGroupTracker trackerGroup
                    ? trackerGroup.TryGetValue((SymbolId)lookupIdentifier, out tracker)
                    : ns.Tracker.TryGetValue((SymbolId)lookupIdentifier, out tracker);
                if (found)
                {
                    if (tracker is TypeGroup typeGroup)
                    {
                        if (hasTypeArguments)
                        {
                            _ = typeArguments.Resolve(rc);
                            TypeTracker genericType = typeGroup.GetTypeForArity(typeArguments.Count);
                            if (genericType != null)
                            {
                                return new TypeExpression(
                                    genericType.Type.MakeGenericType(typeArguments.ResolvedTypes));
                            }

                            // ERROR!
                        }

                        Type nonGenericType;
                        if (typeGroup.TryGetNonGenericType(out nonGenericType))
                        {
                            return new TypeExpression(nonGenericType);
                        }

                        // ERROR!
                    }

                    if (tracker is TypeTracker singleTypeTracker)
                    {
                        if (hasTypeArguments)
                        {
                            _ = typeArguments.Resolve(rc);
                            return new TypeExpression(
                                singleTypeTracker.Type.MakeGenericType(typeArguments.ResolvedTypes));
                        }
                        return new TypeExpression(singleTypeTracker.Type);
                    }

                    if (tracker is NamespaceTracker childNamespaceTracker)
                    {
                        return new NamespaceExpression(childNamespaceTracker, Span);
                    }

                    // ERROR!
                }

                //FullNamedExpression retval = ns.Lookup(rc.Compiler, lookupIdentifier, loc);

                //if (retval == null && !silent)
                //    ns.Error_NamespaceDoesNotExist(loc, lookupIdentifier, rc.Compiler.Report);
                //else if (targs != null)
                //    retval = new GenericTypeExpr(retval.Type, targs, loc).ResolveAsTypeStep(rc, silent);

                return null;
            }

            TypeExpression resolvedAsType = resolved.ResolveAsTypeTerminal(rc, false);
            if (resolvedAsType == null)
            {
                return null;
            }

            Type expressionType = resolvedAsType.Type;
            if (expressionType.IsGenericParameter)
            {
                rc.Compiler.Errors.Add(
                    rc.Compiler.SourceUnit,
                    string.Format(
                        "A nested type cannot be specified through a type parameter '{0}'.",
                        expressionType.Name),
                    resolvedAsType.Span,
                    704,
                    Severity.Error);
                
                return null;
            }

            MemberGroup memberGroup = languageContext.DefaultBinderState.Binder.GetMember(
                MemberRequestKind.Get,
                expressionType,
                Name);

            if (memberGroup != null)
            {
                foreach (TypeTracker nestedType in memberGroup.OfType<TypeTracker>())
                {
                    if (nestedType.IsGenericType != hasTypeArguments)
                    {
                        continue;
                    }

                    if (!hasTypeArguments)
                    {
                        if (nestedType.Type.GetGenericArguments().Length == typeArguments.Count)
                        {
                            return new TypeExpression(nestedType.Type);
                        }

                        // ERROR!
                    }

                    if (nestedType is TypeGroup typeGroup)
                    {
                        TypeTracker matchingType = typeGroup.GetTypeForArity(typeArguments.Count);
                        if (matchingType != null)
                        {
                            return new TypeExpression(matchingType.Type);
                        }

                        // ERROR!
                    }
                }
            }

            //return null;

            Expression memberLookup = MemberLookup(
                rc,
                null,
                expressionType,
                expressionType,
                lookupIdentifier,
                MemberTypes.NestedType,
                BindingFlags.Public | BindingFlags.NonPublic,
                Span);

            if (memberLookup == null)
            {
                if (silent)
                {
                    return null;
                }

                System.Diagnostics.Debugger.Break();
                //Error_IdentifierNotFound(rc, resolved, lookupIdentifier);
                return null;
            }

            TypeExpression typeExpression = memberLookup.ResolveAsTypeTerminal(rc, false);
            if (typeExpression == null)
            {
                return null;
            }

            TypeArguments theArgs = typeArguments;
            Type declaringType = typeExpression.Type.DeclaringType;

            if (TypeManager.HasGenericArguments(declaringType) && !TypeManager.IsGenericTypeDefinition(expressionType))
            {
                while (!TypeManager.IsEqual(TypeManager.DropGenericTypeArguments(expressionType), declaringType))
                {
                    expressionType = expressionType.BaseType;
                }

                TypeArguments newArgs = new TypeArguments();

                foreach (Type decl in expressionType.GetGenericArguments())
                    newArgs.Add(new TypeExpression(decl) { Span = Span });

                if (typeArguments != null)
                {
                    newArgs.Add(typeArguments);
                }

                theArgs = newArgs;
            }

            if (theArgs != null)
            {
                GenericTypeExpression genericType = new GenericTypeExpression(typeExpression.Type, theArgs, Span);
                return genericType.ResolveAsTypeStep(rc, false);
            }

            return typeExpression;
        }
    }
}