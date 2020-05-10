using System.Text;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class LocalDeclaration : Ast
    {
        private Expression _initializer;
        public string VariableName { get; set; }

        public Expression Initializer
        {
            get => _initializer;
            set => _initializer = value;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _initializer, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            StringBuilder declarationBuilder = new StringBuilder();

            _ = declarationBuilder.Append("local ");
            _ = declarationBuilder.Append(VariableName);
            _ = declarationBuilder.Append(" = ");

            sw.Write(declarationBuilder.ToString());

            if (_initializer == null)
            {
                sw.Write("?");
                return;
            }

            int oldIndent = sw.Indent;
            int initializerIndentChange = indentChange;
            Microsoft.Scripting.SourceLocation initializerStart = _initializer.Span.Start;

            if (initializerStart.Line == Span.Start.Line)
            {
                int declarationLength = declarationBuilder.Length;
                int originalInitializerOffset = initializerStart.Column - 1;

                initializerIndentChange += declarationLength - originalInitializerOffset - sw.Indent;
            }
            else
            {
                sw.WriteLine();
                initializerIndentChange = initializerStart.Column - 1 - sw.Indent + indentChange;
            }

            sw.Indent += initializerIndentChange;

            try
            {
                _initializer.Dump(sw, initializerIndentChange);
            }
            finally
            {
                sw.Indent = oldIndent;
            }
        }
    }
}