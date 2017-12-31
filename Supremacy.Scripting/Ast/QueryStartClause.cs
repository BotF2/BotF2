using System;
using System.ComponentModel;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class QueryStartClause : QueryClause
    {
        public RangeDeclaration RangeVariable { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Expression Initializer
        {
            get { return Expression; }
            set { Expression = value; }
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("from ");
            DumpChild(RangeVariable, sw, indentChange);
            sw.Write(" in");

            var initializerIsQuery = Initializer is QueryExpression;
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

        protected override string MethodName
        {
            get { throw new NotSupportedException(); }
        }

        public override void BeginInit(ParseContext parseContext, bool raiseInitialized)
        {
            _queryScope = new QueryScope(
                parseContext.Compiler,
                parseContext.CurrentScope,
                RangeVariable,
                Span.Start);

            base.BeginInit(parseContext, raiseInitialized);
        }

        public override Expression BuildQueryClause(ParseContext ec, Expression leftSide)
        {
            //var oldScope = ec.CurrentScope;
            //ec.CurrentScope = this.Scope;

            //var next = this.Next;

/*
            var nextScope = (QueryScope)next.Scope;
            if (nextScope == null)
            {
                var nextRangeClause = this.Next as RangeVariableQueryClause;
                if (nextRangeClause != null)
                {
                    nextScope = new QueryScope(
                        ec.Compiler,
                        this.Scope,
                        nextRangeClause.RangeVariable,
                        nextRangeClause.Span.Start);
                }
                else
                {
                    nextScope = new QueryScope(
                        ec.Compiler,
                        ec.CurrentScope,
                        next.Span.Start);
                }
                next.Scope = nextScope;
            }
*/

            //((QueryScope)this.Scope).AddTransparentParameter(this.RangeVariable);

            var result = Next.BuildQueryClause(ec, Initializer);

            //ec.CurrentScope = oldScope;

            return result;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            //var oldScope = ec.CurrentScope;
            //ec.CurrentScope = this.Scope;

            var result = BuildQueryClause(ec, null).Resolve(ec);

            //ec.CurrentScope = oldScope;

            return result;
        }
    }
}