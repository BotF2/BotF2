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

        private static bool EnsureParser()
        {
            if (_parser != null)
            {
                return true;
            }

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string grammar = (from name in assembly.GetManifestResourceNames()
                                  where name.EndsWith("ExpressionLanguage.mx")
                                  select name).Single();

                System.IO.Stream stream = assembly.GetManifestResourceStream(grammar);
                if (stream == null)
                {
                    return false;
                }

                using (stream)
                {
                    using (MImage image = new MImage(stream, true))
                    {
                        ParserFactory factory = image.ParserFactories["Supremacy.Scripting.ExpressionLanguage"];
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
            _ = EnsureParser();
            return _parser.Parse<Expression>(source.GetReader(), _xamlTypeMap);
        }

        public MSAst.LambdaExpression ParseScript(
            SourceUnit scriptSource,
            ScriptCompilerOptions compilerOptions,
            ErrorSink errorSink)
        {
            _ = EnsureParser();

            CompilerContext compilerContext = new CompilerContext(scriptSource, compilerOptions, errorSink);
            ScriptGenerator generator = new ScriptGenerator(this, compilerContext);
            Expression expression = ParseExpression(scriptSource);

            LinkParents.Link(expression);

            ParseContext parseContext = new ParseContext(compilerContext)
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
            //foreach (var item in parseContext.CurrentScope.TopLevel.Parameters.)
            //{

            //}
            Console.WriteLine("TopLevel {0}, StartLocation {1}, End {2}, Parent {3}, CompilerContext {4}, Explicit {5}, CompilerErrorCount {6}, CurrentAnonymousMethod {7}, InitilizserVarible {8}, languageContext {9}, IsVariableCapturingRequired {10}", parseContext.Compiler.ToString(),
                parseContext.CurrentScope.TopLevel,
                parseContext.CurrentScope.StartLocation,
                parseContext.CurrentScope.EndLocation,
                parseContext.CurrentScope.Parent,
                parseContext.CurrentScope.CompilerContext,
                parseContext.CurrentScope.Explicit,
                parseContext.CompilerErrorCount,
                parseContext.ConstantCheckState,
                parseContext.CurrentAnonymousMethod,
                parseContext.CurrentInitializerVariable,
                parseContext.LanguageContext,
                parseContext.IsVariableCapturingRequired
                );
            //Console.ReadLine();
            AstInitializer.Initialize(parseContext, ref expression);

            Expression resolved = expression.Resolve(parseContext);

            MSAst.ParameterExpression[] parameters = compilerOptions.Parameters
                .Select(o => generator.Scope.TopScope.GetOrMakeLocal(o.Name, o.Type))
                .ToArray();

            MSAst.Expression transformedBody = resolved.Transform(generator);

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
            NamespaceTracker currentTracker = _topNamespace;
            string[] parts = nsName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (!currentTracker.TryGetValue((SymbolId)part, out MemberTracker memberTracker))
                {
                    return null;
                }

                currentTracker = memberTracker as NamespaceTracker;

                if (currentTracker == null)
                {
                    return null;
                }
            }

            return currentTracker;
        }

        private void LoadAssembly(Assembly assembly)
        {
            if (!_topNamespace.LoadAssembly(assembly))
            {
                return;
            }

            LoadAssemblyScriptImports(assembly);

            IEnumerable<ScriptNamespaceAliasAttribute> aliasAttributes = assembly
                .GetCustomAttributes(typeof(ScriptNamespaceAliasAttribute), false)
                .Cast<ScriptNamespaceAliasAttribute>();

            foreach (ScriptNamespaceAliasAttribute aliasAttribute in aliasAttributes)
            {
                NamespaceTracker namespaceTracker = LookupNamespace(aliasAttribute.Namespace);
                if (namespaceTracker == null)
                {
                    continue;
                }

                lock (_aliasedNamespaceGroups)
                {

                    if (!_aliasedNamespaceGroups.TryGetValue(aliasAttribute.Alias, out NamespaceGroupTracker aliasedNamespaces))
                    {
                        _aliasedNamespaceGroups[aliasAttribute.Alias] = aliasedNamespaces = new NamespaceGroupTracker(aliasAttribute.Alias, _topNamespace);
                    }

                    aliasedNamespaces.IncludeNamespace(namespaceTracker);
                }
            }
        }

        private void LoadAssemblyScriptImports(Assembly assembly)
        {
            ScriptVisibleNamespace[] scriptVisibleNamespaces = assembly.GetScriptVisibleNamespaces();
            foreach (ScriptVisibleNamespace s in scriptVisibleNamespaces)
            {
                NamespaceTracker ns = _topNamespace;
                Assembly loadedAssembly = s.Assembly ?? assembly;

                if (loadedAssembly != null)
                {
                    _ = DomainManager.LoadAssembly(loadedAssembly);
                    _ = _topNamespace.LoadAssembly(loadedAssembly);
                }

                List<string> parts = s.ClrNamespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                while (parts.Count > 0)
                {
                    if (!ns.TryGetValue((SymbolId)parts[0], out MemberTracker mt))
                    {
                        ns = null;
                        break;
                    }
                    ns = (NamespaceTracker)mt;
                    parts.RemoveAt(0);
                }

                if (ns == null)
                {
                    continue;
                }

                ScriptVisibleNamespaces.IncludeNamespace(ns);
            }
        }

        public ScriptLanguageContext(ScriptDomainManager domainManager, IDictionary<string, object> options)
            : base(domainManager)
        {
            _topNamespace = new TopNamespaceTracker(domainManager);
            _aliasedNamespaceGroups = new Dictionary<string, NamespaceGroupTracker>();
            ScriptVisibleNamespaces = new NamespaceGroupTracker("$sxe", _topNamespace);

            if ((_onAssemblyLoadHandler == null) &&
                (Interlocked.CompareExchange(ref _onAssemblyLoadHandler, OnAssemblyLoaded, null) == null))
            {
                DomainManager.AssemblyLoaded += _onAssemblyLoadHandler;
            }

            foreach (Assembly assembly in DomainManager.GetLoadedAssemblyList())
            {
                LoadAssembly(assembly);
            }

            LoadAssemblyScriptImports(typeof(ScriptLanguageContext).Assembly);

            DefaultBinderState = new BinderState(new ScriptBinder(this));
            OverloadResolver = new ScriptOverloadResolverFactory(this);

            _ = EnsureParser();
        }

        public NamespaceGroupTracker ScriptVisibleNamespaces { get; }

        public NamespaceTracker LookupAliasedNamespaceGroup(string alias)
        {
            return _aliasedNamespaceGroups.TryGetValue(alias, out NamespaceGroupTracker alisedNamespaces) ? alisedNamespaces : null;
        }

        internal bool TryResolveType(NameExpression typeName, out Type type, out int errorCode, out string errorMessage)
        {
            errorCode = 0;
            errorMessage = null;
            type = null;

            if (typeName == null)
            {
                return false;
            }

            string normalizedName = ReflectionUtils.GetNormalizedTypeName(typeName.Name);

            lock (ScriptVisibleNamespaces)
            {
                if (!ScriptVisibleNamespaces.TryGetValue((SymbolId)normalizedName, out MemberTracker memberTracker))
                {
                    return false;
                }

                if (memberTracker is TypeGroup typeGroup)
                {

                    int genericArity = typeName.TypeArguments.Count;
                    if (typeGroup.TypesByArity.TryGetValue(genericArity, out Type resolvedType))
                    {
                        type = resolvedType;
                        if (genericArity == 0)
                        {
                            return true;
                        }
                    }
                }

                if (!(memberTracker is TypeTracker typeTracker))
                {
                    return false;
                }

                type = typeTracker.Type;
                return true;
            }
        }

        public ScriptOverloadResolverFactory OverloadResolver { get; }

        public BinderState DefaultBinderState { get; }

        public NamespaceTracker GlobalRootNamespace => _topNamespace;

        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
        {
            MSAst.LambdaExpression transformed = ParseScript(sourceUnit, (ScriptCompilerOptions)options, errorSink);

            return new LegacyScriptCode(
                transformed as MSAst.LambdaExpression ??
                MSAst.Expression.Lambda(transformed),
                sourceUnit);
        }
    }

    public class ScriptCompilerOptions : CompilerOptions
    {
        public ScriptCompilerOptions() : this(new ScriptParameters()) { }

        public ScriptCompilerOptions(ScriptParameters parameters)
        {
            Parameters = parameters ?? throw new ArgumentNullException("parameters");
        }

        public ScriptParameters Parameters { get; }
    }

    public class NamespaceGroupTracker : NamespaceTracker
    {
        private readonly List<NamespaceTracker> _includedNamespaces = new List<NamespaceTracker>();

        public IList<NamespaceTracker> IncludedNamespaces => _includedNamespaces;

        public NamespaceGroupTracker(string alias, TopNamespaceTracker topNamespaceTracker)
            : base(alias)
        {
            SetTopPackage(topNamespaceTracker);
            _includedNamespaces = new List<NamespaceTracker>();
        }

        public void IncludeNamespace(NamespaceTracker includedNamespace)
        {
            if (!_includedNamespaces.Contains(includedNamespace))
            {
                _includedNamespaces.Add(includedNamespace);
            }
        }

        public new bool TryGetValue(SymbolId name, out MemberTracker value)
        {
            foreach (NamespaceTracker includedNamespace in _includedNamespaces)
            {
                if (includedNamespace.TryGetValue(name, out value))
                {
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}