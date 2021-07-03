using System;
using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    internal class ParameterReference : Expression
    {
        public ParameterReference(Parameter parameter)
        {
            Parameter = parameter ?? throw new ArgumentNullException("parameter");
            Type = parameter.ParameterType;
            ExpressionClass = ExpressionClass.Variable;
        }

        public Parameter Parameter { get; private set; }

        public override bool IsPrimaryExpression => true;

        public override System.Linq.Expressions.Expression TransformCore(ScriptGenerator generator)
        {
            return generator.Scope.LookupName(Parameter.Name);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(Parameter.Name);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            return !DoResolveBase(parseContext) ? null : this;
        }

        private bool DoResolveBase(ParseContext ec)
        {
            Type = Parameter.ParameterType;
            ExpressionClass = ExpressionClass.Variable;

            LambdaExpression am = ec.CurrentAnonymousMethod;
            if (am == null)
            {
                return true;
            }

            Scope b = ec.CurrentScope;
            while (b != null)
            {
                b = b.TopLevel;
                IParameterData[] p = b.TopLevel.Parameters.FixedParameters;
                if (p.Any(t => t == Parameter))
                {
                    if (b == ec.CurrentScope.TopLevel)
                    {
                        return true;
                    }

                    if ((Parameter.ModifierFlags & Parameter.Modifier.IsByRef) == Parameter.Modifier.IsByRef)
                    {
                        ec.ReportError(
                            1628,
                            string.Format(
                                "Parameter '{0}' cannot be used inside '{1}' when using 'ref' or 'out' modifier.",
                                Parameter.Name,
                                TypeManager.GetCSharpName(typeof(CompilationUnit))),
                            Span);
                    }

                    return true;
                }

                b = b.Parent;
            }

            return true;
        }
    }
}