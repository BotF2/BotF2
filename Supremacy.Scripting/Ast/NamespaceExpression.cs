using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

namespace Supremacy.Scripting.Ast
{
    public class NamespaceExpression : FullNamedExpression
    {
        public NamespaceExpression(NamespaceTracker tracker) : this(tracker, SourceSpan.None) { }

        public NamespaceExpression(NamespaceTracker tracker, SourceSpan span)
        {
            Tracker = tracker;
            ExpressionClass = ExpressionClass.Namespace;
            Span = span;
        }

        public NamespaceTracker Tracker { get; }
    }
}