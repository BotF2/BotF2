using System;
using System.Collections.Generic;
using System.Dataflow;
using System.Reflection;
using System.Threading;

using Microsoft.M;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using Supremacy.Scripting.Runtime;

using System.Linq;

using Supremacy.Scripting.Ast;
using Supremacy.Scripting.Runtime.Binders;

using MSAst = System.Linq.Expressions;
using CompilerOptions = Microsoft.Scripting.CompilerOptions;
using SourceSpan = Microsoft.Scripting.SourceSpan;
using ReflectionUtils = Microsoft.Scripting.Utils.ReflectionUtils;

using MGraphXamlReader;

[assembly: ScriptVisibleNamespace("System", "mscorlib")]
[assembly: ScriptVisibleNamespace("System.Collections", "mscorlib")]
[assembly: ScriptVisibleNamespace("System.Collections.Generic", "mscorlib")]
[assembly: ScriptVisibleNamespace("System.Linq", "System.Core")]
[assembly: ScriptVisibleNamespace("System.Linq.Expressions", "System.Core")]

namespace Supremacy.Scripting.Runtime
{
    public class ScriptLanguageContext : LanguageContext
    {
        private static Parser _parser;
        private static Dictionary<Identifier, Type> _xamlTypeMap;
        private static readonly Dictionary<string, TypeTracker> _typeGroups = new Dictionary<string, TypeTracker>();
        
        private readonly HashSet<string> _defines = new HashSet<string>();
        private readonly TopNamespaceTracker _topNamespace;
        private readonly EventHandler<AssemblyLoadedEventArgs> _onAssemblyLoadHandler;
        private readonly Dictionary<string, NamespaceGroupTracker> _aliasedNamespaceGroups;
        private readonly NamespaceGroupTracker _scriptVisibleNamespaces;

        private readonly BinderState _defaultBinderState;
        private readonly ScriptOverloadResolverFactory _overloadResolver;

        private static bool EnsureParser()
        {
            if (_parser != null)
                return true;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var grammar = (from name in assembly.GetManifestResourceNames()
                               where name.EndsWith("ExpressionLanguage.mx")
                               select name).Single();

                var stream = assembly.GetManifestResourceStream(grammar);
                if (stream == null)
                    return false;

                using (stream)
                {
                    using (var image = new MImage(stream, true))
                    {
                        var factory = image.ParserFactories["Supremacy.Scripting.ExpressionLanguage"];
                        _parser = factory.Create();
                    }
                }

                _xamlTypeMap = assembly
                    .GetTypes()
                    .Where(o => o.Namespace == "Supremacy.Scripting.Ast" && o.IsPublic && !o.IsAbstract)
                    .ToDictionary(o => (Identifier)o.Name, o => o);

                return true;
            }
            catch
            {
                throw;
            }
        }

        internal Expression ParseExpression(SourceUnit source)
        {
            EnsureParser();
            return _parser.Parse<Expression>(source.GetReader(), _xamlTypeMap);
        }

        public MSAst.LambdaExpression ParseScript(
            SourceUnit scriptSource, 
            ScriptCompilerOptions compilerOptions,
            ErrorSink errorSink)
        {
            EnsureParser();

            var compilerContext = new CompilerContext(scriptSource, compilerOptions, errorSink);
            var generator = new ScriptGenerator(this, compilerContext);
            var expression = ParseExpression(scriptSource);

            LinkParents.Link(expression);

            var parseContext = new ParseContext(compilerContext)
                               {
                                   CurrentScope = new TopLevelScope(
                                       compilerContext,
                                       new ParametersCompiled(
                                           compilerOptions.Parameters.Select(
                                               o => new Parameter(
                                                        o.Name,
                                                        null,
                                                        SourceSpan.None)
                                                    {
                                                        ParameterType = o.Type
                                                    })),
                                       expression.Span.Start)
                               };

            AstInitializer.Initialize(parseContext, ref expression);

            var resolved = expression.Resolve(parseContext);

            var parameters = compilerOptions.Parameters
                .Select(o => generator.Scope.TopScope.GetOrMakeLocal(o.Name, o.Type))
                .ToArray();

            var transformedBody = resolved.Transform(generator);

            return MSAst.Expression.Lambda(
                transformedBody,
                parameters);
        }

        private void OnAssemblyLoaded(object sender, AssemblyLoadedEventArgs args)
        {
            LoadAssembly(args.Assembly);
        }

        public bool IsConditionalDefined(string name)
        {
            return _defines.Contains(name);
        }

        private NamespaceTracker LookupNamespace(string nsName)
        {
            var currentTracker = (NamespaceTracker)_topNamespace;
            var parts = nsName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                if (!currentTracker.TryGetValue((SymbolId)part, out MemberTracker memberTracker))
                    return null;

                currentTracker = memberTracker as NamespaceTracker;
                
                if (currentTracker == null)
                    return null;
            }

            return currentTracker;
        }

        private void LoadAssembly(Assembly assembly)
        {
            if (!_topNamespace.LoadAssembly(assembly))
                return;

            LoadAssemblyScriptImports(assembly);
            
            var aliasAttributes = assembly
                .GetCustomAttributes(typeof(ScriptNamespaceAliasAttribute), false)
                .Cast<ScriptNamespaceAliasAttribute>();
            
            foreach (var aliasAttribute in aliasAttributes)
            {
                var namespaceTracker = LookupNamespace(aliasAttribute.Namespace);
                if (namespaceTracker == null)
                    continue;

                lock (_aliasedNamespaceGroups)
                {
                    NamespaceGroupTracker aliasedNamespaces;

                    if (!_aliasedNamespaceGroups.TryGetValue(aliasAttribute.Alias, out aliasedNamespaces))
                        _aliasedNamespaceGroups[aliasAttribute.Alias] = aliasedNamespaces = new NamespaceGroupTracker(aliasAttribute.Alias, _topNamespace);
                    
                    aliasedNamespaces.IncludeNamespace(namespaceTracker);
                }
            }
        }

        private void LoadAssemblyScriptImports(Assembly assembly) {
            var scriptVisibleNamespaces = assembly.GetScriptVisibleNamespaces();
            foreach (var s in scriptVisibleNamespaces)
            {
                var ns = (NamespaceTracker)_topNamespace;
                var loadedAssembly = s.Assembly ?? assembly;

                if (loadedAssembly != null)
                {
                    DomainManager.LoadAssembly(loadedAssembly);
                    _topNamespace.LoadAssembly(loadedAssembly);
                }

                var parts = s.ClrNamespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                while (parts.Count > 0)
                {
                    MemberTracker mt;
                    if (!ns.TryGetValue((SymbolId)parts[0], out mt))
                    {
                        ns = null;
                        break;
                    }
                    ns = (NamespaceTracker)mt;
                    parts.RemoveAt(0);
                }

                if (ns == null)
                    continue;

                _scriptVisibleNamespaces.IncludeNamespace(ns);
            }
        }

        public ScriptLanguageContext(ScriptDomainManager domainManager, IDictionary<string,object> options)
            : base(domainManager)
        {
            _topNamespace = new TopNamespaceTracker(domainManager);
            _aliasedNamespaceGroups = new Dictionary<string, NamespaceGroupTracker>();
            _scriptVisibleNamespaces = new NamespaceGroupTracker("$sxe", _topNamespace);

            if ((_onAssemblyLoadHandler == null) && 
                (Interlocked.CompareExchange(ref _onAssemblyLoadHandler, OnAssemblyLoaded, null) == null))
            {
                DomainManager.AssemblyLoaded += _onAssemblyLoadHandler;
            }

            foreach (var assembly in DomainManager.GetLoadedAssemblyList())
                LoadAssembly(assembly);

            LoadAssemblyScriptImports(typeof(ScriptLanguageContext).Assembly);

            _defaultBinderState = new BinderState(new ScriptBinder(this));
            _overloadResolver = new ScriptOverloadResolverFactory(this);

            EnsureParser();
        }

        public NamespaceGroupTracker ScriptVisibleNamespaces
        {
            get { return _scriptVisibleNamespaces; }
        }

        public NamespaceTracker LookupAliasedNamespaceGroup(string alias)
        {
            NamespaceGroupTracker alisedNamespaces;
            return _aliasedNamespaceGroups.TryGetValue(alias, out alisedNamespaces) ? alisedNamespaces : null;
        }

        internal bool TryResolveType(NameExpression typeName, out Type type, out int errorCode, out string errorMessage)
        {
            errorCode = 0;
            errorMessage = null;
            type = null;

            if (typeName == null)
                return false;

            var normalizedName = ReflectionUtils.GetNormalizedTypeName(typeName.Name);

            lock (_scriptVisibleNamespaces)
            {
                MemberTracker memberTracker;

                if (!_scriptVisibleNamespaces.TryGetValue((SymbolId)normalizedName, out memberTracker))
                    return false;

                var typeGroup = memberTracker as TypeGroup;
                if (typeGroup != null)
                {
                    Type resolvedType;

                    var genericArity = typeName.TypeArguments.Count;
                    if (typeGroup.TypesByArity.TryGetValue(genericArity, out resolvedType))
                    {
                        type = resolvedType;
                        if (genericArity == 0)
                            return true;
                    }
                }

                var typeTracker = memberTracker as TypeTracker;
                if (typeTracker == null)
                    return false;

                type = typeTracker.Type;
                return true;
            }
        }

        public ScriptOverloadResolverFactory OverloadResolver
        {
            get { return _overloadResolver; }
        }

        public BinderState DefaultBinderState
        {
            get { return _defaultBinderState; }
        }

        public NamespaceTracker GlobalRootNamespace
        {
            get { return _topNamespace; }
        }

        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
        {
            var transformed = ParseScript(sourceUnit, (ScriptCompilerOptions)options, errorSink);

            return new LegacyScriptCode(
                transformed as MSAst.LambdaExpression ??
                MSAst.Expression.Lambda(transformed),
                sourceUnit);
        }
    }

    public class ScriptCompilerOptions : CompilerOptions
    {
        private readonly ScriptParameters _parameters;

        public ScriptCompilerOptions() : this(new ScriptParameters()) { }

        public ScriptCompilerOptions(ScriptParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            _parameters = parameters;
        }

        public ScriptParameters Parameters
        {
            get { return _parameters; }
        }
    }

    public class NamespaceGroupTracker : NamespaceTracker
    {
        private readonly List<NamespaceTracker> _includedNamespaces = new List<NamespaceTracker>();

        public IList<NamespaceTracker> IncludedNamespaces
        {
            get { return _includedNamespaces; }
        }

        public NamespaceGroupTracker(string alias, TopNamespaceTracker topNamespaceTracker)
            : base(alias)
        {
            SetTopPackage(topNamespaceTracker);
            _includedNamespaces = new List<NamespaceTracker>();
        }

        public void IncludeNamespace(NamespaceTracker includedNamespace)
        {
            if (!_includedNamespaces.Contains(includedNamespace))
                _includedNamespaces.Add(includedNamespace);
        }

        public new bool TryGetValue(SymbolId name, out MemberTracker value)
        {
            foreach (var includedNamespace in _includedNamespaces)
            {
                if (includedNamespace.TryGetValue(name, out value))
                    return true;
            }
            value = null;
            return false;
        }
    }
}