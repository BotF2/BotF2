using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System;

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
            {
                parent.AddAnonymousChild(this);
            }

            if ((Parameters != null) && !Parameters.IsEmpty)
            {
                ProcessParameters();
            }
            //Console.WriteLine("...doing TopLevelScope");
        }

        public TopLevelScope(CompilerContext ctx, SourceLocation loc)
            : this(ctx, null, ParametersCompiled.EmptyReadOnlyParameters, loc) { }

        public ParametersCompiled Parameters { get; set; }

        public TopLevelScope Container => Parent?.TopLevel;

        protected void ProcessParameters()
        {
            //Console.WriteLine("...ProcessParameters of TopLevelScope");
            int count = Parameters.Count;
            TopLevelScope topParent = Parent?.TopLevel;
            TopLevelParameterInfo[] parameterInfo = new TopLevelParameterInfo[count];

            for (int i = 0; i < count; ++i)
            {
                parameterInfo[i] = new TopLevelParameterInfo(this, i);

                Parameter p = Parameters[i];
                if (p == null)
                {
                    continue;
                }

                if (p.Scope == null)
                {
                    p.Scope = this;
                }

                string name = p.Name;
                //Console.WriteLine("...ProcessParameters of TopLevelScope: " + name);

                if (CheckParentConflictName(topParent, name, p.Span))
                {
                    AddKnownVariable(name, parameterInfo[i]);
                }
            }
        }

        public Expression GetParameterReference(string name, SourceSpan span)
        {
            for (TopLevelScope topLevelScope = this; topLevelScope != null; topLevelScope = topLevelScope.Container)
            {
                Expression expression = topLevelScope.GetParameterReferenceExpression(name, span);
                if (expression != null)
                {
                    return expression;
                }
            }
            return null;
        }

        protected virtual Expression GetParameterReferenceExpression(string name, SourceSpan span)
        {
            if (Parameters == null)
            {
                return null;
            }

            int index = Parameters.GetParameterIndexByName(name);
            return index < 0 ? null : new ParameterReference(Parameters[index]);
        }
    }
}