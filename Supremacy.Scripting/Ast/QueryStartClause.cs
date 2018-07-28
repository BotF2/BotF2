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
            var result = Next.BuildQueryClause(ec, Initializer);
            return result;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            var result = BuildQueryClause(ec, null).Resolve(ec);
            return result;
        }
    }
}