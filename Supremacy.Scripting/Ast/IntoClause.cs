using System;
using System.ComponentModel;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class IntoClause : RangeVariableQueryClause
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public QueryStartClause Initializer
        {
            get => Expression as QueryStartClause;
            set => Expression = value;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            DumpChild(Initializer, sw, indentChange);
            sw.Write(" into ");
            DumpChild(RangeVariable, sw, indentChange);
            sw.WriteLine();
            sw.Write(' ', Span.Start.Column - 1);
            DumpChild(Next, sw, indentChange);
        }

        protected override string MethodName => throw new NotSupportedException();

        public override void BeginInit(ParseContext parseContext, bool raiseInitialized)
        {
            base.BeginInit(parseContext, raiseInitialized);

            if (Scope == null)
            {
                Scope = new QueryScope(
                    parseContext.Compiler,
                    parseContext.CurrentScope,
                    RangeVariable,
                    Span.Start);
            }

            if (Initializer.Scope == null)
            {
                Initializer.Scope = new QueryScope(
                    parseContext.Compiler,
                    parseContext.CurrentScope,
                    Initializer.RangeVariable,
                    Initializer.Span.Start);
            }

            parseContext.CurrentScope = Scope;
        }

        public override void EndInit(ParseContext parseContext)
        {
            parseContext.CurrentScope = parseContext.CurrentScope.Parent;

            base.EndInit(parseContext);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            //var currentScope = ec.CurrentScope;
            //ec.CurrentScope = this.Scope;

            Expression nestedFrom = new QueryStartClause
                             {
                                 RangeVariable = RangeVariable,
                                 Initializer = Initializer,
                                 Next = Next,
                                 Scope = Scope
                             }.DoResolve(ec);

            //ec.CurrentScope = currentScope;

            return nestedFrom;
        }
    }
}