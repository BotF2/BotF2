using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public class ImplicitLambdaParameter : Parameter
    {
        public ImplicitLambdaParameter(string name, Scope scope, SourceSpan span)
            : base(name, scope, span) { }
    }
}