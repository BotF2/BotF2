using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   This class is used to "construct" the type during a typecast
    ///   operation.  Since the Type.GetType class in .NET can parse
    ///   the type specification, we just use this to construct the type
    ///   one bit at a time.
    /// </summary>
    public class ComposedCastExpression : TypeExpression
    {
        private FullNamedExpression _left;

        public ComposedCastExpression() { }

        public ComposedCastExpression(FullNamedExpression left, string dimensionSpecifier)
            : this(left, dimensionSpecifier, left.Span) { }

        public ComposedCastExpression(FullNamedExpression left, string dimensionSpecifier, SourceSpan span)
        {
            Left = left;
            DimensionSpecifier = dimensionSpecifier;
            Span = span;
        }

        public string DimensionSpecifier { get; set; }

        public FullNamedExpression Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo<T>(cloneContext, target);

            var clone = target as ComposedCastExpression;
            if (clone == null)
                return;

            clone.Left = Clone(cloneContext, Left);
            clone.DimensionSpecifier = DimensionSpecifier;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _left, prefix, postfix);
        }

        protected override TypeExpression DoResolveAsTypeStep(ParseContext ec)
        {
            var leftExpression = Left.ResolveAsTypeTerminal(ec, false);
            if (leftExpression == null)
                return null;

            var leftType = leftExpression.Type;
            if ((DimensionSpecifier.Length > 0) && (DimensionSpecifier[0] == '?'))
            {
                TypeExpression nullable = new NullableTypeExpression(leftExpression, Span);
                if (DimensionSpecifier.Length > 1)
                    nullable = new ComposedCastExpression(nullable, DimensionSpecifier.Substring(1), Span);
                return nullable.ResolveAsTypeTerminal(ec, false);
            }

            if (DimensionSpecifier.Length != 0 && DimensionSpecifier[0] == '[')
            {
                if (TypeManager.IsSpecialType(leftType))
                {
                    ec.ReportError(
                        611,
                        string.Format(
                            "Array elements cannot be of type '{0}'.",
                            TypeManager.GetCSharpName(leftType)),
                        Span);

                    return null;
                }

                if (leftType.IsAbstract && leftType.IsSealed)
                {
                    ec.ReportError(
                        719,
                        string.Format(
                            "Array elements cannot be of static type `{0}'",
                            TypeManager.GetCSharpName(leftType)),
                        Span);
                }
            }

            if (DimensionSpecifier != "")
                Type = TypeManager.GetConstructedType(leftType, DimensionSpecifier);
            else
                Type = leftType;

            if (Type == null)
                throw new InternalErrorException("Couldn't create computed type " + leftType + DimensionSpecifier);

            ExpressionClass = ExpressionClass.Type;
            return this;
        }

        public override string GetSignatureForError()
        {
            return Left.GetSignatureForError() + DimensionSpecifier;
        }

        public override TypeExpression ResolveAsTypeTerminal(ParseContext ec, bool silent)
        {
            return ResolveAsBaseTerminal(ec, silent);
        }
    }
}