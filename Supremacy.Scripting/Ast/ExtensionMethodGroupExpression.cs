using System;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class ExtensionMethodGroupExpression : MethodGroupExpression
    {
        private Argument _extensionArgument;
        private Expression _extensionExpression;

        public ExtensionMethodGroupExpression(MemberInfo[] members, Type type, SourceSpan span, bool inacessibleCandidatesOnly) : base(members, type, span, inacessibleCandidatesOnly) {}
        public ExtensionMethodGroupExpression(MemberInfo[] members, Type type, SourceSpan span) : base(members, type, span) {}

        internal ExtensionMethodGroupExpression()
        {
            // For cloning purposes only.
        }

        public Expression ExtensionExpression
        {
            get { return _extensionExpression; }
            set { _extensionExpression = value; }
        }

        public override bool IsStatic
        {
            get { return true; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Walk(ref _extensionArgument, prefix, postfix);
            Walk(ref _extensionExpression, prefix, postfix);
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as ExtensionMethodGroupExpression;
            if (clone == null)
                return;

            clone._extensionArgument = Clone(cloneContext, _extensionArgument);
            clone.ExtensionExpression = Clone(cloneContext, ExtensionExpression);
        }

        public override MethodGroupExpression OverloadResolve(ParseContext ec, ref Arguments arguments, bool mayFail, SourceSpan location)
        {
            if (arguments == null)
                arguments = new Arguments(1);

            arguments.Insert(0, new Argument(ExtensionExpression));
            var mg = ResolveOverloadExtensions(ec, ref arguments, location);

            // Store resolved argument and restore original arguments
            if (mg != null)
                ((ExtensionMethodGroupExpression)mg)._extensionArgument = arguments[0];
            else
                arguments.RemoveAt(0);	// Clean-up modified arguments for error reporting

            return mg;
        }

        private MethodGroupExpression ResolveOverloadExtensions(ParseContext ec, ref Arguments arguments, SourceSpan location)
        {
            // Use normal resolve rules
            var mg = base.OverloadResolve(ec, ref arguments, true, location);
            if (mg != null)
                return mg;

            return null;
/*
            if (ns == null)
                return null;

            // Search continues
            ExtensionMethodGroupExpr e = ec.LookupExtensionMethod(this.ExpressionType, Name, location);
            if (e == null)
                return base.OverloadResolve(ec, ref arguments, false, location);

            e.ExtensionExpression = ExtensionExpression;
            e.SetTypeArguments(ec, type_arguments);
            return e.ResolveOverloadExtensions(ec, ref arguments, e.namespace_entry, location);
*/
        }		
    }
}