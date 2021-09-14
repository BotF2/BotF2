using System.ComponentModel;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public abstract class QueryClause : ShimExpression
    {
        public TopLevelScope Scope { get; set; }

        public override void BeginInit(ParseContext parseContext, bool raiseInitialized)
        {
            base.BeginInit(parseContext, false);

            if (_queryScope == null)
            {
                _queryScope = new QueryScope(
                    parseContext.Compiler,
                    parseContext.CurrentScope,
                    Span.Start);
            }

            if (Scope == null)
            {
                Scope = _queryScope.TopLevel;
            }

            parseContext.CurrentScope = _queryScope;

            if (raiseInitialized)
            {
                OnInitialized();
            }
        }

        public override void EndInit(ParseContext parseContext)
        {
            if (parseContext.CurrentScope == _queryScope)
            {
                parseContext.CurrentScope = parseContext.CurrentScope.Parent;
                _queryScope = null;
            }

            base.EndInit(parseContext);
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is QueryClause targetQueryClause))
            {
                return;
            }

            if (Scope != null)
            {
                targetQueryClause.Scope = Scope.Clone<TopLevelScope>(cloneContext);
            }

            if (Next != null)
            {
                targetQueryClause.Next = Clone(cloneContext, Next);
            }
        }

        public override Expression DoResolve(ParseContext ec)
        {
            return Expression.DoResolve(ec);
        }

        public virtual Expression BuildQueryClause(ParseContext ec, Expression leftSide)
        {

            CreateArguments(ec, out Arguments arguments);

            leftSide = CreateQueryExpression(leftSide, arguments);

            if (Next != null)
            {
                if ((!(Next is SelectClause selectClause)) || selectClause.IsRequired)
                {
                    return Next.BuildQueryClause(ec, leftSide);
                }

                // Skip transparent select clause if any clause follows
                if (Next.Next != null)
                {
                    return Next.Next.BuildQueryClause(ec, leftSide);
                }
            }

            return leftSide;
        }

        protected virtual void CreateArguments(ParseContext ec, out Arguments args)
        {
            args = new Arguments(2);

            LambdaExpression selector = new LambdaExpression
            {
                Scope = Scope,
                Body = Expression
            };

            _ = args.Add(new Argument(selector));
        }

        protected Expression CreateQueryExpression(Expression target, Arguments arguments)
        {
            QueryExpressionInvocation invocation = new QueryExpressionInvocation
            {
                Target = new QueryExpressionAccess
                {
                    Name = MethodName,
                    Left = target
                }
            };
            invocation.Arguments.AddRange(arguments);
            return invocation;
        }

        protected Expression CreateQueryExpression(Expression lSide, TypeArguments typeArguments, Arguments arguments)
        {
            QueryExpressionInvocation e = new QueryExpressionInvocation
            {
                Target = new QueryExpressionAccess
                {
                    Left = lSide,
                    Name = MethodName,
                    Span = Span
                }
            };

            e.TypeArguments.Add(typeArguments);
            e.Arguments.AddRange(arguments);

            return e;
        }

        protected abstract string MethodName { get; }

        private QueryClause _next;
        protected QueryScope _queryScope;

        public virtual QueryClause Next
        {
            get => _next;
            set => _next = value;
        }

        public QueryClause Tail => (Next == null) ? this : Next.Tail;

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Walk(ref _next, prefix, postfix);
        }

        internal class QueryExpressionAccess : MemberAccessExpression { }

        internal class QueryExpressionInvocation : InvokeExpression, MethodGroupExpression.IErrorHandler
        {
            protected override MethodGroupExpression DoResolveOverload(ParseContext ec)
            {
                MethodGroup.CustomErrorHandler = this;
                Arguments arguments = Arguments;
                MethodGroupExpression rmg = MethodGroup.OverloadResolve(ec, ref arguments, false, Span);
                Arguments = arguments;
                return rmg;
            }

            public bool AmbiguousCall(ParseContext ec, MethodBase ambiguous)
            {
                ec.ReportError(
                    1940,
                    string.Format(
                        "Ambiguous implementation of the query pattern '{0}' for source type '{1}'.",
                        MethodGroup.Name,
                        MethodGroup.InstanceExpression.GetSignatureForError()),
                    Span);

                return true;
            }

            public bool NoExactMatch(ParseContext ec, MethodBase method)
            {
                ParametersCollection pd = TypeManager.GetParameterData(method);
                System.Type sourceType = pd.ExtensionMethodType;
                if (sourceType != null)
                {
                    Argument a = Arguments[0];

                    if (TypeManager.IsGenericType(sourceType) && TypeManager.ContainsGenericParameters(sourceType))
                    {
                        TypeInferenceContext tic = new TypeInferenceContext(sourceType.GetGenericArguments());
                        _ = tic.OutputTypeInference(ec, a.Value, sourceType);
                        if (tic.FixAllTypes(ec))
                        {
                            sourceType = TypeManager.DropGenericTypeArguments(sourceType).MakeGenericType(tic.InferredTypeArguments);
                        }
                    }

                    if (!TypeManager.ImplicitConversionExists(ec, a.Value, sourceType))
                    {
                        ec.ReportError(
                            1936,
                            string.Format(
                                "An implementation of `{0}' query expression pattern for source type `{1}' could not be found",
                                MethodGroup.Name,
                                TypeManager.GetCSharpName(a.Value.Type)),
                            Span);

                        return true;
                    }
                }

                if (!method.IsGenericMethod)
                {
                    return false;
                }

                if (MethodGroup.Name == "SelectMany")
                {
                    ec.ReportError(
                        1943,
                        string.Format(
                            "An expression type is incorrect in a subsequent 'from' clause in a query expression with source type '{0}'.",
                            Arguments[0].GetSignatureForError()),
                        Span);
                }
                else
                {
                    ec.ReportError(
                        1942,
                        string.Format(
                            "An expression type in '{0}' clause is incorrect. Type inference failed in the call to '{1}'.",
                            MethodGroup.Name.ToLower(),
                            MethodGroup.Name),
                        Span);
                }

                return true;
            }

            public new QueryExpressionAccess Target
            {
                get => base.Target as QueryExpressionAccess;
                set => base.Target = value;
            }
        }
    }

    public class AnonymousMemberDeclarator : ShimExpression
    {
        public string Name { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Expression Initializer
        {
            get => Expression;
            set => Expression = value;
        }

        public AnonymousMemberDeclarator(Expression initializer, string name, SourceSpan span)
        {
            Expression = initializer;
            Name = name;
            Span = span;
        }

        public AnonymousMemberDeclarator(Parameter parameter)
        {
            Expression = new NameExpression { Name = parameter.Name, Span = parameter.Span };
            Name = parameter.Name;
            Span = parameter.Span;
        }

        public override bool Equals(object o)
        {
            return (o is AnonymousMemberDeclarator other) && (other.Name == Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if ((Expression is LiteralExpression literal) && (literal.Kind == LiteralKind.Null))
            {
                OnInvalidInitializerError(ec, Expression.GetType().Name);
                return null;
            }

            if (Name == null)
            {
                if (Expression is NameExpression)
                {
                    Name = ((NameExpression)Expression).Name;
                }
                else if (Expression is MemberAccessExpression)
                {
                    Name = ((MemberAccessExpression)Expression).Name;
                }
                else if (Expression is MemberInitializerExpression)
                {
                    Name = ((MemberInitializerExpression)Expression).MemberName;
                }
                else
                {
                    OnInvalidInitializerError(ec, Expression.GetType().Name);
                }
            }

            return Initializer = Initializer.Resolve(ec);
        }

        protected virtual void OnInvalidInitializerError(ParseContext ec, string initializer)
        {
            ec.Compiler.Errors.Add(
                ec.Compiler.SourceUnit,
                string.Format(
                    "An anonymous type property '{0}' cannot be initialized with '{1}'.",
                    Name,
                    initializer),
                Span,
                828,
                Severity.Error);
        }
    }
}