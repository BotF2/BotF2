using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public interface ISourceLocation
    {
        string FileName { get; set; }
        SourceSpan Span { get; set; }
    }
}