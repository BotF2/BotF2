using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class QualifiedAliasMember : MemberAccessExpression
    {
        public static readonly string GlobalAlias = "global";

        public QualifiedAliasMember() {}

        public QualifiedAliasMember(string alias, string name, SourceSpan span)
        {
            Alias = alias;
            Name = name;
            Span = span;
        }

        public string Alias { get; set; }

        public override Expression DoResolve(ParseContext ec)
        {
            return ResolveAsTypeStep(ec, false);
        }

        public override FullNamedExpression ResolveAsTypeStep(ParseContext ec, bool silent)
        {
            if (Alias == GlobalAlias)
            {
                Left = new NamespaceExpression(ec.LanguageContext.GlobalRootNamespace);
                return base.ResolveAsTypeStep(ec, silent);
            }

            var errorCount = ec.CompilerErrorCount;
            Left = ec.LookupNamespaceAlias(Alias);
            
            if (Left == null)
            {
                if (errorCount == ec.CompilerErrorCount)
                {
                    ec.ReportError(
                        432,
                        string.Format("Alias '{0}' not found.", Alias),
                        Span);
                }
                return null;
            }

            var fullNamedExpression = base.ResolveAsTypeStep(ec, silent);
            if (fullNamedExpression == null)
                return null;

            if (Left.ExpressionClass == ExpressionClass.Type)
            {
                if (!silent)
                {
                    ec.ReportError(
                        431,
                        string.Format(
                            "Alias '{0}' cannot be used with '::' since it denotes a type.  Consider replacing '::' with '.'.",
                            Alias),
                        Span);
                }
                return null;
            }

            return fullNamedExpression;
        }
    }
}