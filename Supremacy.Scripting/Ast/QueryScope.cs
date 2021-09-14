using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class QueryScope : TopLevelScope
    {
        //
        // Transparent parameters are used to package up the intermediate results
        // and pass them onto next clause
        //
        public sealed class TransparentParameter : ImplicitLambdaParameter
        {
            public static int Counter;
            private const string ParameterNamePrefix = "*";

            public readonly ParametersCompiled Parent;
            public readonly string Identifier;

            public TransparentParameter(QueryScope scope, ParametersCompiled parent, RangeDeclaration identifier)
                : base(ParameterNamePrefix + Counter++, scope, identifier.Span)
            {
                Parent = parent;
                Identifier = identifier.VariableName;
            }

            public static void Reset()
            {
                Counter = 0;
            }
        }

        public sealed class ImplicitQueryParameter : ImplicitLambdaParameter
        {
            public ImplicitQueryParameter(string name, Scope scope, SourceSpan span)
                : base(name, scope, span)
            {
                Name = name;
                Span = span;
            }
        }

        public QueryScope(CompilerContext ctx, Scope parent, RangeDeclaration lt, SourceLocation startLocation)
            : base(ctx, parent, null, startLocation)
        {
            Parameters = new ParametersCompiled(
                new ImplicitQueryParameter(
                    lt.VariableName,
                    this,
                    lt.Span));

            ProcessParameters();

            if (parent != null)
            {
                _ = base.CheckParentConflictName(parent.TopLevel, lt.VariableName, lt.Span);
            }
        }

        public QueryScope(CompilerContext ctx, Scope parent, ParametersCompiled parameters, RangeDeclaration lt, SourceLocation startLocation)
            : base(ctx, parent, null, startLocation)
        {
            Parameters = new ParametersCompiled(
                parameters[0],
                new ImplicitQueryParameter(
                    lt.VariableName,
                    this,
                    lt.Span));

            ProcessParameters();
        }

        public QueryScope(CompilerContext ctx, Scope parent, SourceLocation startLocation)
            : base(ctx, parent, parent.TopLevel.Parameters, startLocation)
        {
        }

        public void AddTransparentParameter(RangeDeclaration name)
        {
            _ = base.CheckParentConflictName(this, name.VariableName, name.Span);

            Parameters = new ParametersCompiled(new TransparentParameter(this, Parameters, name));
        }

        protected override bool CheckParentConflictName(TopLevelScope block, string name, SourceSpan span)
        {
            return true;
        }

        //
        // Query parameter reference can include transparent parameters
        //
        protected override Expression GetParameterReferenceExpression(string name, SourceSpan span)
        {
            Expression reference = base.GetParameterReferenceExpression(name, span);
            if (reference != null)
            {
                return reference;
            }

            TransparentParameter transparentParameter = Parameters[0] as TransparentParameter;
            while (transparentParameter != null)
            {
                if (transparentParameter.Identifier == name)
                {
                    break;
                }

                TransparentParameter nextTransparentParameter = transparentParameter.Parent[0] as TransparentParameter;
                if (nextTransparentParameter == null)
                {
                    if (transparentParameter.Parent.GetParameterIndexByName(name) >= 0)
                    {
                        break;
                    }
                }

                transparentParameter = nextTransparentParameter;
            }

            if (transparentParameter != null)
            {
                reference = new NameExpression
                {
                    Name = Parameters[0].Name,
                    Span = span
                };

                TransparentParameter transparentParameterCursor = (TransparentParameter)Parameters[0];
                while (transparentParameterCursor != transparentParameter)
                {
                    transparentParameterCursor = (TransparentParameter)transparentParameterCursor.Parent[0];
                    reference = new MemberAccessExpression
                    {
                        Left = reference,
                        Name = transparentParameterCursor.Name
                    };
                }

                return new MemberAccessExpression
                {
                    Left = reference,
                    Name = name
                };
            }

            return null;
        }
    }
}