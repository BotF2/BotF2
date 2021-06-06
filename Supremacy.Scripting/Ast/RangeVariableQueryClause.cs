using System;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public abstract class RangeVariableQueryClause : QueryClause
    {
        private RangeDeclaration _rangeVeriable;

        private sealed class RangeAnonymousMemberDeclarator : AnonymousMemberDeclarator
        {
            public RangeAnonymousMemberDeclarator(Expression initializer, RangeDeclaration parameter)
                : base(initializer, parameter.VariableName, parameter.Span)
            {
            }

            protected override void OnInvalidInitializerError(ParseContext ec, string initializer)
            {
                ec.Compiler.Errors.Add(
                    ec.Compiler.SourceUnit,
                    string.Format(
                        "An range variable '{0}' cannot be initialized with '{1}'.",
                        Name,
                        initializer),
                    Span,
                    828,
                    Severity.Error);
            }
        }

        protected RangeVariableQueryClause() { }

        protected RangeVariableQueryClause(TopLevelScope scope, Expression baseExpression)
        {
            Scope = scope;
            Expression = baseExpression;
            Span = baseExpression.Span;
        }

        public RangeDeclaration RangeVariable
        {
            get => _rangeVeriable;
            set => _rangeVeriable = value;
        }

        protected static Expression CreateRangeVariableType(
            TopLevelScope scope,
            ParseContext parseContext,
            RangeDeclaration declaration,
            Expression initializer)
        {
            AnonymousObjectInitializer anonInitializer = new AnonymousObjectInitializer
            {
                FileName = declaration.FileName,
                Span = initializer.Span,
            };
            anonInitializer.MemberDeclarators.Add(new AnonymousMemberDeclarator(scope.Parameters[0]));
            anonInitializer.MemberDeclarators.Add(
                new RangeAnonymousMemberDeclarator(
                    initializer,
                    declaration));
            return new AnonymousObjectCreationExpression
            {
                FileName = declaration.FileName,
                Span = declaration.Span,
                Initializer = anonInitializer
            };
        }

        public override void BeginInit(ParseContext parseContext, bool raiseInitialized)
        {
            _queryScope = new QueryScope(
                parseContext.Compiler,
                parseContext.CurrentScope,
                Span.Start);

            Next.Initialized += OnNextInitialized;

            base.BeginInit(parseContext, raiseInitialized);
        }

        private void OnNextInitialized(object sender, EventArgs eventArgs)
        {
            Next.Initialized -= OnNextInitialized;
            ((QueryScope)Next.Scope).AddTransparentParameter(RangeVariable);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Walk(ref _rangeVeriable, prefix, postfix);
        }
    }
}