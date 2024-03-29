using System.Collections.Generic;
using System.Linq;
using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class ParameterizedExpression : ShimExpression
    {
        private readonly List<LocalDeclaration> _locals = new List<LocalDeclaration>();

        public IList<LocalDeclaration> Locals => _locals;

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            WalkList(_locals, prefix, postfix);

            base.Walk(prefix, postfix);
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            Expression expression = Expression;
            if (expression == null)
            {
                return MSAst.Default(typeof(object));
            }

            LocalDeclaration[] locals = _locals.ToArray();
            ScriptScope scope = generator.PushNewScope();

            try
            {
                MSAst[] localInitializers = locals.Select(o => o.Initializer.Transform(generator)).ToArray();
                //var existingParamCount = generator.Scope.Parent.Locals.Count();

                foreach (LocalDeclaration local in locals)
                {
                    _ = scope.CreateParameter(local.VariableName);
                }

                MSAst lambdaBody = expression.Transform(generator);
                System.Linq.Expressions.LambdaExpression innerLambda = scope.FinishScope(lambdaBody);

                return MSAst.Invoke(
                    innerLambda,
                    localInitializers);
            }
            finally
            {
                //generator.PopScope();
                generator.PopScope();
            }
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            foreach (LocalDeclaration local in _locals)
            {
                DumpChild(local, sw, indentChange);
                sw.WriteLine();
            }

            DumpChild(Expression ?? new NullLiteral(Span), sw, indentChange);
        }

        public Scope Scope { get; set; }

        public override void BeginInit(ParseContext parseContext, bool raiseInitialized)
        {
            base.BeginInit(parseContext, false);

            if (Scope == null)
            {
                ParametersCompiled parameters = new ParametersCompiled(
                    Locals.Select(o => new Parameter(o.VariableName, null, o.Span)));

                Scope = new TopLevelScope(
                    parseContext.Compiler,
                    ParametersCompiled.MergeGenerated(
                        parameters,
                        true,
                        (Parameter[])parseContext.CurrentScope.TopLevel.Parameters.FixedParameters,
                        parseContext.CurrentScope.TopLevel.Parameters.Types),
                    Span.Start);

                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].Scope = Scope;
                }
            }

            parseContext.CurrentScope = Scope;

            if (raiseInitialized)
            {
                OnInitialized();
            }
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            Expression = Expression.Resolve(parseContext);
            return this;
        }
    }
}