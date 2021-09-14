using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public interface IAst : ISourceLocation
    {
        void CloneTo<T>(CloneContext cloneContext, T target) where T : class, IAst;
        T GetEnclosingAst<T>() where T : class, IAst;

        void Walk(AstVisitor prefix, AstVisitor postfix);

        IAst ParentAst { get; set; }

        void Dump(SourceWriter sw, int indentChange);
        void BeginInit(ParseContext parseContext, bool raiseInitialized);
        void EndInit(ParseContext parseContext);
    }
}