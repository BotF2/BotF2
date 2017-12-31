using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public interface IKnownVariable
    {
        Scope Scope { get; }
        SourceSpan Span { get; }
    }
}