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
            get { return _initializer; }
            set { _initializer = value; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _initializer, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            var declarationBuilder = new StringBuilder();

            declarationBuilder.Append("local ");
            declarationBuilder.Append(VariableName);
            declarationBuilder.Append(" = ");

            sw.Write(declarationBuilder.ToString());

            if (_initializer == null)
            {
                sw.Write("?");
                return;
            }

            var oldIndent = sw.Indent;
            var initializerIndentChange = indentChange;
            var initializerStart = _initializer.Span.Start;

            if (initializerStart.Line == Span.Start.Line)
            {
                var declarationLength = declarationBuilder.Length;
                var originalInitializerOffset = initializerStart.Column - 1;

                initializerIndentChange += (declarationLength - originalInitializerOffset) - sw.Indent;
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