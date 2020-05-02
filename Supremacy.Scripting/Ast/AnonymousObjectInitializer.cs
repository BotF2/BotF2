using System.Collections.Generic;
using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class AnonymousObjectInitializer : Expression
    {
        private readonly List<AnonymousMemberDeclarator> _memberDeclarators = new List<AnonymousMemberDeclarator>();

        public IList<AnonymousMemberDeclarator> MemberDeclarators => _memberDeclarators;

        public override Expression DoResolve(ParseContext parseContext)
        {
            foreach (AnonymousMemberDeclarator memberDeclarator in _memberDeclarators)
            {
                _ = memberDeclarator.Resolve(parseContext);
            }

            return this;
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is AnonymousObjectInitializer initializer))
            {
                return;
            }

            foreach (AnonymousMemberDeclarator declarator in _memberDeclarators)
            {
                initializer._memberDeclarators.Add(Clone(cloneContext, declarator));
            }
        }
    }

    public class AnonymousObjectCreationExpression : Expression
    {
        private AnonymousObjectInitializer _initializer;

        public AnonymousObjectInitializer Initializer
        {
            get => _initializer;
            set => _initializer = value;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _initializer, prefix, postfix);
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is AnonymousObjectCreationExpression clone))
            {
                return;
            }

            clone._initializer = Clone(cloneContext, _initializer);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            _ = _initializer.Resolve(parseContext);

            AnonymousTypeClass anonymousType = TypeManager.GetAnonymousType(_initializer);

            return new NewInitExpression(
                new TypeExpression(anonymousType.Type),
                new Arguments(),
                _initializer.MemberDeclarators.Select(
                    o => new NewInitMemberBinding(
                             anonymousType[o.Name],
                             o.Initializer))
                    .ToArray(),
                Span).Resolve(parseContext);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("new ");

            if (Initializer != null)
            {
                DumpChild(Initializer, sw);
            }
        }
    }
}