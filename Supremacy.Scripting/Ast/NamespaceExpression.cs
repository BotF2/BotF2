using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

namespace Supremacy.Scripting.Ast
{
    public class NamespaceExpression : FullNamedExpression
    {
        private readonly NamespaceTracker _tracker;

        public NamespaceExpression(NamespaceTracker tracker) : this(tracker, SourceSpan.None) { }

        public NamespaceExpression(NamespaceTracker tracker, SourceSpan span)
        {
            _tracker = tracker;
            ExpressionClass = ExpressionClass.Namespace;
            Span = span;
        }

        public NamespaceTracker Tracker
        {
            get { return _tracker; }
        }
    }
}