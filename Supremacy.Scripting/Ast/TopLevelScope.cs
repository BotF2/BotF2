using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class TopLevelScope : ExplicitScope
    {
        public TopLevelScope(CompilerContext ctx, ParametersCompiled parameters, SourceLocation start)
            : this(ctx, null, parameters, start) { }

        // We use 'Parent' to hook up to the containing Scope, but don't want to register the current Scope as a child.
        // So, we use a two-stage setup -- first pass a null parent to the base constructor, and then override 'Parent'.
        public TopLevelScope(CompilerContext ctx, Scope parent, ParametersCompiled parameters, SourceLocation start)
            : base(ctx, parent, start, SourceLocation.None)
        {
            TopLevel = this;
            Parameters = parameters;
            Parent = parent;

            if (parent != null)
                parent.AddAnonymousChild(this);

            if ((Parameters != null) && !Parameters.IsEmpty)
                ProcessParameters();
        }

        public TopLevelScope(CompilerContext ctx, SourceLocation loc)
            : this(ctx, null, ParametersCompiled.EmptyReadOnlyParameters, loc) { }

        public ParametersCompiled Parameters { get; set; }

        public TopLevelScope Container
        {
            get { return (Parent == null) ? null : Parent.TopLevel; }
        }

        protected void ProcessParameters()
        {
            var count = Parameters.Count;
            var topParent = (Parent == null) ? null : Parent.TopLevel;
            var parameterInfo = new TopLevelParameterInfo[count];

            for (var i = 0; i < count; ++i)
            {
                parameterInfo[i] = new TopLevelParameterInfo(this, i);

                var p = Parameters[i];
                if (p == null)
                    continue;

                if (p.Scope == null)
                    p.Scope = this;

                var name = p.Name;

                if (CheckParentConflictName(topParent, name, p.Span))
                    AddKnownVariable(name, parameterInfo[i]);
            }
        }

        public Expression GetParameterReference(string name, SourceSpan span)
        {
            for (var topLevelScope = this; topLevelScope != null; topLevelScope = topLevelScope.Container)
            {
                var expression = topLevelScope.GetParameterReferenceExpression(name, span);
                if (expression != null)
                    return expression;
            }
            return null;
        }

        protected virtual Expression GetParameterReferenceExpression(string name, SourceSpan span)
        {
            if (Parameters == null)
                return null;

            var index = Parameters.GetParameterIndexByName(name);
            if (index < 0)
                return null;

            return new ParameterReference(Parameters[index]);
        }
    }
}