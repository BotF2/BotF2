using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class UsingEntry
    {
        public FullNamedExpression Name { get; set; }

        protected FullNamedExpression Resolved { get; set; }

        public UsingEntry Clone(CloneContext cloneContext)
        {
            return new UsingEntry
            {
                Name = Ast.Clone(cloneContext, Name),
                Resolved = Ast.Clone(cloneContext, Resolved)
            };
        }

        public string GetSignatureForError()
        {
            return Name.GetSignatureForError();
        }

        public override string ToString()
        {
            return GetSignatureForError();
        }

        public virtual FullNamedExpression Resolve(ParseContext rc)
        {
            if (Resolved != null)
            {
                return Resolved as NamespaceExpression;
            }

            Resolved = Name is MemberAccessExpression memberAccess
                ? memberAccess.ResolveNamespaceOrType(rc, false)
                : Name.Resolve(rc) as FullNamedExpression;

            if (Resolved == null)
            {
                rc.ReportError(
                    138,
                    string.Format(
                        "'{0}' is a type, not a namespace.  A using namespace directive can only be applied to namespaces.",
                        GetSignatureForError()),
                    Name.Span);
            }

            return Resolved;
        }
    }

    public class UsingAliasEntry : UsingEntry
    {
        public string Alias { get; set; }

        public override FullNamedExpression Resolve(ParseContext rc)
        {
            if (Resolved != null || Name == null)
            {
                return Resolved;
            }

            if (rc == null)
            {
                return null;
            }

            _ = base.Resolve(rc);

            if (Resolved == null)
            {
                Name = null;
                return null;
            }

            if (Resolved is TypeExpression)
            {
                Resolved = Resolved.ResolveAsBaseTerminal(rc, false);
            }

            return Resolved;
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", Alias, Name.GetSignatureForError());
        }
    }
}