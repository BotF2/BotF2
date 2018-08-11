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
            get { return _left; }
            set { _left = value; }
        }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

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
            var parenthesize = !_left.IsPrimaryExpression;

            if (parenthesize)
                sw.Write("(");

            _left.Dump(sw, indentChange);

            if (parenthesize)
                sw.Write(")");

            sw.Write(".");
            sw.Write(Name);

            if (HasTypeArguments)
            {
                sw.Write("<");
                for (var i = 0; i < TypeArguments.Count; i++)
                {
                    var type = TypeArguments[i];
                    if (i != 0)
                        sw.Write(",");
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

        public MemberAccessExpression() {}

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
                return _resolved;

            var type = Type;
            if (type != null)
                throw new Exception();

            //
            // Resolve the expression with flow analysis turned off, we'll do the definite
            // assignment checks later.  This is because we don't know yet what the expression
            // will resolve to - it may resolve to a FieldExpr and in this case we must do the
            // definite assignment check on the actual field and not on the whole struct.
            //

            var original = Left as NameExpression;
            
            var leftResolved = Left.Resolve(ec,
                ResolveFlags.VariableOrValue | ResolveFlags.Type |
                ResolveFlags.Intermediate | ResolveFlags.DisableStructFlowAnalysis);

            if (leftResolved == null)
                return null;

            var typeArguments = TypeArguments;
            var lookupIdentifier = MemberName.MakeName(Name, typeArguments);

            var ns = leftResolved as NamespaceExpression;
            if (ns != null)
            {
                FullNamedExpression resolved = null;

                MemberTracker namespaceChild;

                bool found;

                var trackerGroup = ns.Tracker as NamespaceGroupTracker;
                if (trackerGroup != null)
                    found = trackerGroup.TryGetValue((SymbolId)lookupIdentifier, out namespaceChild);
                else
                    found = ns.Tracker.TryGetValue((SymbolId)lookupIdentifier, out namespaceChild);

                if (!found)
                {
                    // TODO: ns.Error_NamespaceDoesNotExist(loc, lookupIdentifier, ec.Report);
                }
                else if (HasTypeArguments)
                {
                    if (!typeArguments.Resolve(ec))
                        return null;

                    var typeTracker = namespaceChild as TypeGroup;
                    if (typeTracker != null)
                    {
                        var matchingType = typeTracker.GetTypeForArity(typeArguments.Count);
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
                    var typeGroup = namespaceChild as TypeGroup;
                    if (typeGroup != null)
                    {
                        Type nonGenericType;
                        if (typeGroup.TryGetNonGenericType(out nonGenericType))
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
                        var nestedType = namespaceChild as NestedTypeTracker;
                        if (nestedType != null)
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
                    ExpressionClass = resolved.ExpressionClass;
                return resolved;
            }

            var expressionType = leftResolved.Type;

            if (expressionType.IsPointer || expressionType == TypeManager.CoreTypes.Void)
            {
                // TODO: Unary.Error_OperatorCannotBeApplied(ec, loc, ".", expressionType);
                return null;
            }

            var c = leftResolved as ConstantExpression;
            if (c != null && c.Value == null)
            {
                ec.ReportError(
                    1720,
                    "Expression will always cause a 'System.NullReferenceException'.",
                    Severity.Warning,
                    Span);
            }

            if (typeArguments != null && !typeArguments.Resolve(ec))
                return null;

            var memberLookup = MemberLookup(
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
                var expressionClass = leftResolved.ExpressionClass;

                //
                // Extension methods are not allowed on all expression types
                //
                if (expressionClass == ExpressionClass.Value || expressionClass == ExpressionClass.Variable ||
                    expressionClass == ExpressionClass.IndexerAccess || expressionClass == ExpressionClass.PropertyAccess ||
                    expressionClass == ExpressionClass.EventAccess)
                {
                    var exMethodLookup = ec.LookupExtensionMethod(expressionType, Name, Span);
                    if (exMethodLookup != null)
                    {
                        exMethodLookup.ExtensionExpression = leftResolved;

                        if ((typeArguments != null) && typeArguments.Any())
                            exMethodLookup.SetTypeArguments(ec, typeArguments);

                        _resolved = exMethodLookup.DoResolve(ec);
                        if (_resolved != null)
                            ExpressionClass = _resolved.ExpressionClass;
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
                    return null;
            }

            var texpr = memberLookup as TypeExpression;
            if (texpr != null)
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

                var ct = leftResolved as GenericTypeExpression;
                if (ct != null)
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
                        ExpressionClass = _resolved.ExpressionClass;
                    return _resolved;
                }

                _resolved = memberLookup;
                ExpressionClass = _resolved.ExpressionClass;
                return memberLookup;
            }

            var me = (MemberExpression)memberLookup;
            me = me.ResolveMemberAccess(ec, leftResolved, Span, original);
            if (me == null)
                return null;

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

            if (rightSide != null)
                return (_resolved = me.DoResolveLValue(ec, rightSide));
            return (_resolved = me.DoResolve(ec));
        }

        public override FullNamedExpression ResolveAsTypeStep(ParseContext ec, bool silent)
        {
            var result = ResolveNamespaceOrType(ec, silent);
            Type = result.Type;
            return result;
        }

        public FullNamedExpression ResolveNamespaceOrType(ParseContext rc, bool silent)
        {
            var resolved = Left.ResolveAsTypeStep(rc, silent);

            if (resolved == null)
                return null;

            var languageContext = (ScriptLanguageContext)rc.Compiler.SourceUnit.LanguageContext;
            
            var hasTypeArguments = HasTypeArguments;
            var typeArguments = TypeArguments;

            var lookupIdentifier = ReflectionUtils.GetNormalizedTypeName(MemberName.MakeName(Name, typeArguments));

            var ns = resolved as NamespaceExpression;
            
            if (ns != null)
            {
                MemberTracker tracker;

                bool found;

                var trackerGroup = ns.Tracker as NamespaceGroupTracker;
                if (trackerGroup != null)
                    found = trackerGroup.TryGetValue((SymbolId)lookupIdentifier, out tracker);
                else
                    found = ns.Tracker.TryGetValue((SymbolId)lookupIdentifier, out tracker);

                if (found)
                {
                    var typeGroup = tracker as TypeGroup;
                    if (typeGroup != null)
                    {
                        if (hasTypeArguments)
                        {
                            typeArguments.Resolve(rc);
                            var genericType = typeGroup.GetTypeForArity(typeArguments.Count);
                            if (genericType != null)
                            {
                                return new TypeExpression(
                                    genericType.Type.MakeGenericType(typeArguments.ResolvedTypes));
                            }

                            // ERROR!
                        }

                        Type nonGenericType;
                        if (typeGroup.TryGetNonGenericType(out nonGenericType))
                            return new TypeExpression(nonGenericType);

                        // ERROR!
                    }

                    var singleTypeTracker = tracker as TypeTracker;
                    if (singleTypeTracker != null)
                    {
                        if (hasTypeArguments)
                        {
                            typeArguments.Resolve(rc);
                            return new TypeExpression(
                                singleTypeTracker.Type.MakeGenericType(typeArguments.ResolvedTypes));
                        }
                        return new TypeExpression(singleTypeTracker.Type);
                    }

                    var childNamespaceTracker = tracker as NamespaceTracker;
                    if (childNamespaceTracker != null)
                        return new NamespaceExpression(childNamespaceTracker, Span);

                    // ERROR!
                }

                //FullNamedExpression retval = ns.Lookup(rc.Compiler, lookupIdentifier, loc);

                //if (retval == null && !silent)
                //    ns.Error_NamespaceDoesNotExist(loc, lookupIdentifier, rc.Compiler.Report);
                //else if (targs != null)
                //    retval = new GenericTypeExpr(retval.Type, targs, loc).ResolveAsTypeStep(rc, silent);

                return null;
            }

            var resolvedAsType = resolved.ResolveAsTypeTerminal(rc, false);
            if (resolvedAsType == null)
                return null;

            var expressionType = resolvedAsType.Type;
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

            var memberGroup = languageContext.DefaultBinderState.Binder.GetMember(
                MemberRequestKind.Get,
                expressionType,
                Name);

            if (memberGroup != null)
            {
                foreach (var nestedType in memberGroup.OfType<TypeTracker>())
                {
                    if (nestedType.IsGenericType != hasTypeArguments)
                        continue;

                    if (!hasTypeArguments)
                    {
                        if (nestedType.Type.GetGenericArguments().Length == typeArguments.Count)
                            return new TypeExpression(nestedType.Type);

                        // ERROR!
                    }

                    var typeGroup = nestedType as TypeGroup;
                    if (typeGroup != null)
                    {
                        var matchingType = typeGroup.GetTypeForArity(typeArguments.Count);
                        if (matchingType != null)
                            return new TypeExpression(matchingType.Type);

                        // ERROR!
                    }
                }
            }

            //return null;

            var memberLookup = MemberLookup(
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
                    return null;

                System.Diagnostics.Debugger.Break();
                //Error_IdentifierNotFound(rc, resolved, lookupIdentifier);
                return null;
            }

            var typeExpression = memberLookup.ResolveAsTypeTerminal(rc, false);
            if (typeExpression == null)
                return null;

            var theArgs = typeArguments;
            var declaringType = typeExpression.Type.DeclaringType;

            if (TypeManager.HasGenericArguments(declaringType) && !TypeManager.IsGenericTypeDefinition(expressionType))
            {
                while (!TypeManager.IsEqual(TypeManager.DropGenericTypeArguments(expressionType), declaringType))
                    expressionType = expressionType.BaseType;

                var newArgs = new TypeArguments();

                foreach (var decl in expressionType.GetGenericArguments())
                    newArgs.Add(new TypeExpression(decl) { Span = Span });

                if (typeArguments != null)
                    newArgs.Add(typeArguments);

                theArgs = newArgs;
            }

            if (theArgs != null)
            {
                var genericType = new GenericTypeExpression(typeExpression.Type, theArgs, Span);
                return genericType.ResolveAsTypeStep(rc, false);
            }

            return typeExpression;
        }
    }
}