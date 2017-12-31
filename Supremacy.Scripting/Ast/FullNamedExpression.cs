using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   Represents a namespace or a type.  The name of the class was inspired by
    ///   section 10.8.1 (Fully Qualified Names).
    /// </summary>
    public abstract class FullNamedExpression : Expression
    {
        // ReSharper disable RedundantOverridenMember
        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);
            // Do nothing, most unresolved type expressions cannot be resolved to different type.
        }
        // ReSharper restore RedundantOverridenMember

        public override FullNamedExpression ResolveAsTypeStep(ParseContext ec, bool silent)
        {
            return this;
        }
    }
}