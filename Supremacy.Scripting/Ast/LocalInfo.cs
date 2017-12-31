using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public class LocalInfo : IKnownVariable
    {
        public string Name { get; private set; }
        public SourceSpan Span { get; private set; }
        public Scope Scope { get; private set; }

        public LocalInfo(string name, Scope scope, SourceSpan span)
        {
            Name = name;
            Scope = scope;
            Span = span;
        }

        public LocalInfo Clone(CloneContext cloneContext)
        {
            return new LocalInfo(Name, cloneContext.LookupBlock(Scope), Span);
        }
    }
}