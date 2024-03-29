using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using Supremacy.Scripting.Ast;
using Supremacy.Scripting.Utility;
using System.Linq;

namespace Supremacy.Scripting.Runtime
{
    [Flags]
    public enum ResolveFlags
    {
        // Returns Value, Variable, PropertyAccess, EventAccess or IndexerAccess.
        VariableOrValue = 1,

        // Returns a type expression.
        Type = 1 << 1,

        // Returns a method group.
        MethodGroup = 1 << 2,

        TypeParameter = 1 << 3,

        // Mask of all the expression class flags.
        MaskExprClass = VariableOrValue | Type | MethodGroup | TypeParameter,

        // Disable control flow analysis while resolving the expression.
        // This is used when resolving the instance expression of a field expression.
        DisableFlowAnalysis = 1 << 10,

        // Set if this is resolving the first part of a MemberAccess.
        Intermediate = 1 << 11,

        // Disable control flow analysis _of struct_ while resolving the expression.
        // This is used when resolving the instance expression of a field expression.
        DisableStructFlowAnalysis = 1 << 12,
    }

    public class ParseContext : IMemberContext
    {
        #region Options Enumeration
        [Flags]
        public enum Options
        {
            /// <summary>
            ///   This flag tracks the `checked' state of the compilation,
            ///   it controls whether we should generate code that does overflow
            ///   checking, or if we generate code that ignores overflows.
            ///
            ///   The default setting comes from the command line option to generate
            ///   checked or unchecked code plus any source code changes using the
            ///   checked/unchecked statements or expressions.   Contrast this with
            ///   the ConstantCheckState flag.
            /// </summary>
            CheckedScope = 1 << 0,

            /// <summary>
            ///   The constant check state is always set to `true' and cant be changed
            ///   from the command line.  The source code can change this setting with
            ///   the `checked' and `unchecked' statements and expressions. 
            /// </summary>
            ConstantCheckState = 1 << 1,

            AllCheckStateFlags = CheckedScope | ConstantCheckState,

            // Indicates the current context is in probing mode, no errors are reported. 
            ProbingMode = 1 << 22,

            // Return and ContextualReturn statements will set the ReturnType
            // value based on the expression types of each return statement
            // instead of the method return type which is initially null.
            InferReturnType = 1 << 23,
            OmitDebuggingInfo = 1 << 24,
            ExpressionTreeConversion = 1 << 25,
            InvokeSpecialName = 1 << 26
        }
        #endregion

        #region FlagsHandle Struct
        public struct FlagsHandle : IDisposable
        {
            private readonly ParseContext _context;
            private readonly Options _invalidMask;
            private readonly Options _oldValue;

            public FlagsHandle(ParseContext context, Options flagsToSet)
                : this(context, flagsToSet, flagsToSet) { }

            internal FlagsHandle(ParseContext context, Options mask, Options val)
            {
                _context = context;
                _invalidMask = ~mask;
                _oldValue = context._flags & mask;
                context._flags = (context._flags & _invalidMask) | (val & mask);
            }

            public void Dispose()
            {
                _context._flags = (_context._flags & _invalidMask) | _oldValue;
            }
        }
        #endregion

        private readonly List<NamespaceTracker> _importedNamespaces;
        private readonly Dictionary<string, FullNamedExpression> _registeredAliases;
        private Options _flags;

        public LambdaExpression CurrentAnonymousMethod { get; set; }
        public Expression CurrentInitializerVariable { get; set; }
        public Ast.Scope CurrentScope { get; set; }

        public ParseContext([Annotations.NotNull] CompilerContext compiler)
        {
            Compiler = compiler ?? throw new ArgumentNullException("compiler");
            LanguageContext = (ScriptLanguageContext)compiler.SourceUnit.LanguageContext;
            _importedNamespaces = new List<NamespaceTracker>();
            _registeredAliases = new Dictionary<string, FullNamedExpression>();
        }

        public ParseContext(Options options, [Annotations.NotNull] CompilerContext compiler)
            : this(compiler)
        {
            if (compiler == null)
            {
                throw new ArgumentNullException("compiler");
            }

            _flags |= options;
        }

        public CompilerContext Compiler { get; }

        public bool IsInProbingMode => (_flags & Options.ProbingMode) != 0;

        public bool IsVariableCapturingRequired => !IsInProbingMode;

        public bool MustCaptureVariable(IKnownVariable variable)
        {
            return CurrentAnonymousMethod == null ? false : variable.Scope.TopLevel != CurrentScope.TopLevel;
        }

        public bool HasSet(Options options)
        {
            return (_flags & options) == options;
        }

        public bool HasAny(Options options)
        {
            return (_flags & options) != 0;
        }

        public FlagsHandle Set(Options options)
        {
            return new FlagsHandle(this, options);
        }

        public FlagsHandle With(Options options, bool enable)
        {
            return new FlagsHandle(this, options, enable ? options : 0);
        }

        [DebuggerStepThrough]
        public void ReportError(int errorCode, string errorMessage, Severity errorSeverity)
        {
            ReportError(errorCode, errorMessage, errorSeverity, SourceSpan.None);
        }

        [DebuggerStepThrough]
        public void ReportError(int errorCode, string errorMessage)
        {
            ReportError(errorCode, errorMessage, Severity.Error, SourceSpan.None);
        }

        [DebuggerStepThrough]
        public void ReportError(int errorCode, string errorMessage, SourceSpan span)
        {
            ReportError(errorCode, errorMessage, Severity.Error, span);
        }

        [DebuggerStepThrough]
        public void ReportError(int errorCode, string errorMessage, Severity errorSeverity, SourceSpan span)
        {
            Compiler.Errors.Add(
                Compiler.SourceUnit,
                errorMessage,
                span,
                errorCode,
                errorSeverity);
        }

        public void ThrowError(int errorCode, string errorMessage, Severity errorSeverity, SourceSpan span)
        {
            throw new SyntaxErrorException(
                errorMessage,
                Compiler.SourceUnit,
                span,
                errorCode,
                errorSeverity);
        }

        [DebuggerStepThrough]
        public void ReportError(Ast.ErrorInfo error, SourceSpan span, params object[] messageArgs)
        {
            Compiler.Errors.Add(
                Compiler.SourceUnit,
                error.FormatMessage(messageArgs),
                span,
                error.Code,
                error.Severity);
        }

        public void ThrowError(Ast.ErrorInfo error, SourceSpan span, params object[] messageArgs)
        {
            throw new SyntaxErrorException(
                error.FormatMessage(messageArgs),
                Compiler.SourceUnit,
                span,
                error.Code,
                error.Severity);
        }

        public int CompilerErrorCount => ((ErrorCounter)Compiler.Errors).ErrorCount;

        public ScriptLanguageContext LanguageContext { get; }

        public bool ConstantCheckState => (_flags & Options.ConstantCheckState) == Options.ConstantCheckState;

        public void ImportNamespace(NamespaceTracker importedNamespace)
        {
            if (!_importedNamespaces.Contains(importedNamespace))
            {
                _importedNamespaces.Add(importedNamespace);
            }
        }

        public void AddUsing([Annotations.NotNull] UsingEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (entry is UsingAliasEntry aliasEntry)
            {
                AddUsingAlias(aliasEntry);
                return;
            }

            if ((entry.Resolve(this) is NamespaceExpression @namespace) && !_importedNamespaces.Contains(@namespace.Tracker))
            {
                _importedNamespaces.Add(@namespace.Tracker);
            }
        }

        public void AddUsingAlias([Annotations.NotNull] UsingAliasEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            FullNamedExpression resolved = entry.Resolve(this);
            if ((resolved != null) && !_registeredAliases.ContainsKey(entry.Alias))
            {
                _registeredAliases[entry.Alias] = resolved;
            }
        }

        public bool IsTypeVisible(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            if (LanguageContext.ScriptVisibleNamespaces.TryGetValue((SymbolId)type.Name, out MemberTracker memberTracker))
            {
                if ((memberTracker is TypeGroup typeGroup) && typeGroup.Types.Contains(type))
                {
                    return true;
                }

                TypeTracker typeTracker = memberTracker as TypeTracker;
                if (typeTracker.Type == type)
                {
                    return true;
                }
            }

            return _importedNamespaces.Any(o => string.Equals(o.Name, type.Namespace, StringComparison.Ordinal));
        }

        #region Implementation of IMemberContext
        public ExtensionMethodGroupExpression LookupExtensionMethod(Type extensionType, string name, SourceSpan location)
        {
            Type openExtendedType = TypeManager.DropGenericTypeArguments(extensionType);

            IList<Type> extendingTypes = LanguageContext.DefaultBinderState.Binder.GetExtensionTypes(openExtendedType);

            MemberInfo[] members =
                (
                    from extendingType in extendingTypes
                    where IsTypeVisible(extendingType)
                    let memberCache = TypeHandle.GetMemberCache(extendingType)
                    from MemberInfo method in memberCache.FindExtensionMethods(
                        thisAssembly: extensionType.Assembly,
                        extensionType: openExtendedType,
                        name: name,
                        publicOnly: true)
                    select method
                ).ToArray();


            return members.Length == 0
                ? null
                : new ExtensionMethodGroupExpression(
                members,
                extensionType,
                location);
        }

        private void OnErrorAmbiguousTypeReference(SourceSpan location, string name, FullNamedExpression t1, FullNamedExpression t2)
        {
            ReportError(
                104,
                string.Format(
                    "'{0}' is an ambiguous reference between '{1}' and '{2}'.",
                    name,
                    t1.GetSignatureForError(),
                    t2.GetSignatureForError()),
                location);
        }

        private FullNamedExpression LookupNamespaceOrType(NamespaceTracker ns, string name, SourceSpan location, bool ignoreAmbiguousReferences, int genericArity = 0)
        {
            MemberTracker tracker;
            FullNamedExpression fne = null;

            bool found = ns is NamespaceGroupTracker trackerGroup
                ? trackerGroup.TryGetValue((SymbolId)name, out tracker)
                : ns.TryGetValue((SymbolId)name, out tracker);
            if (found)
            {
                fne = tracker is NamespaceTracker namespaceTracker
                    ? new NamespaceExpression(namespaceTracker)
                    : (FullNamedExpression)(tracker is TypeGroup typeGroup
                        ? new TypeExpression(typeGroup.GetTypeForArity(genericArity).Type, location)
                        : new TypeExpression(((TypeTracker)tracker).Type));
            }

            if (fne != null)
            {
                return fne;
            }

            //
            // Check using entries.
            //
            IEnumerable<FullNamedExpression> conflicts = _importedNamespaces
                .Select(importedNamespace => LookupNamespaceOrType(importedNamespace, name, location, true))
                .Where(match => (match != null) && (match is TypeExpression));

            foreach (FullNamedExpression conflict in conflicts)
            {
                if (fne != null)
                {
                    if (!ignoreAmbiguousReferences)
                    {
                        OnErrorAmbiguousTypeReference(location, name, fne, conflict);
                    }

                    return null;
                }

                fne = conflict;
            }

            return fne;
        }

        public FullNamedExpression LookupNamespaceOrType(string name, SourceSpan location, bool ignoreAmbiguousReferences, int genericArity = 0)
        {

            if (_registeredAliases.TryGetValue(name, out FullNamedExpression match))
            {
                return match;
            }

            if (LanguageContext.GlobalRootNamespace.TryGetValue((SymbolId)name, out MemberTracker rootNamespaceMember))
            {
                if (rootNamespaceMember is NamespaceTracker topLevelNamespace)
                {
                    return new NamespaceExpression(topLevelNamespace);
                }
            }

            return _importedNamespaces
                .Concat(LanguageContext.ScriptVisibleNamespaces.IncludedNamespaces)
                .Distinct()
                .Select(ns => LookupNamespaceOrType(ns, name, location, ignoreAmbiguousReferences, genericArity))
.FirstOrDefault(result => result != null);
        }

        public FullNamedExpression LookupNamespaceAlias(string alias)
        {
            if (_registeredAliases.TryGetValue(alias, out FullNamedExpression aliasTarget))
            {
                return aliasTarget;
            }

            NamespaceTracker namespaceTracker = LanguageContext.LookupAliasedNamespaceGroup(alias);
            return namespaceTracker == null ? null : new NamespaceExpression(namespaceTracker);
        }
        #endregion
    }
}