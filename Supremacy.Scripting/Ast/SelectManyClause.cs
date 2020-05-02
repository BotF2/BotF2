using System.ComponentModel;
using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class SelectManyClause : RangeVariableQueryClause
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Expression Initializer
        {
            get => Expression;
            set => Expression = value;
        }

        public SelectManyClause() { }

        public SelectManyClause(TopLevelScope scope, RangeDeclaration lt, Expression expr)
            : base(scope, expr)
        {
            RangeVariable = lt;
        }

        protected override void CreateArguments(ParseContext ec, out Arguments args)
        {
            base.CreateArguments(ec, out args);
            
            Expression resultSelectorExpr;

            //
            // When select follow use is as result selector
            //
            if (Next is SelectClause)
            {
                resultSelectorExpr = ((SelectClause)Next).Projection;
                Next = Next.Next;
            }
            else
            {
                
                resultSelectorExpr = CreateRangeVariableType(
                    Scope, 
                    ec,
                    RangeVariable,
                    new NameExpression { Name = RangeVariable.VariableName, Span = RangeVariable.Span });
            }

            LambdaExpression resultSelector = new LambdaExpression
            {
                Span = RangeVariable.Span,
                Scope = new QueryScope(
                                         ec.Compiler,
                                         Scope.Parent,
                                         Scope.TopLevel.Parameters,
                                         RangeVariable,
                                         Scope.StartLocation),
                Body = resultSelectorExpr
            };

            _ = args.Add(new Argument(resultSelector));
        }

        protected override string MethodName => "SelectMany";

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("from ");
            DumpChild(RangeVariable, sw, indentChange);
            sw.Write(" in");

            bool initializerIsQuery = Initializer is QueryExpression;
            if (initializerIsQuery)
            {
                sw.WriteLine();
                sw.Indent += 4;
            }
            else
            {
                sw.Write(' ');
            }

            try
            {
                DumpChild(Initializer, sw, indentChange + 4);
            }
            finally
            {
                if (initializerIsQuery)
                    sw.Indent -= 4;
            }

            sw.WriteLine();
            DumpChild(Next, sw, indentChange);
        }
    }
}