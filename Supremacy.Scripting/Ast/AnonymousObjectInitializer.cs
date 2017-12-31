using System.Collections.Generic;
using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class AnonymousObjectInitializer : Expression
    {
        private readonly List<AnonymousMemberDeclarator> _memberDeclarators = new List<AnonymousMemberDeclarator>();

        public IList<AnonymousMemberDeclarator> MemberDeclarators
        {
            get { return _memberDeclarators; }
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            foreach (var memberDeclarator in _memberDeclarators)
                memberDeclarator.Resolve(parseContext);
            
            return this;
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var initializer = target as AnonymousObjectInitializer;
            if (initializer == null)
                return;

            foreach (var declarator in _memberDeclarators)
                initializer._memberDeclarators.Add(Clone(cloneContext, declarator));
        }
    }

    public class AnonymousObjectCreationExpression : Expression
    {
        private AnonymousObjectInitializer _initializer;

        public AnonymousObjectInitializer Initializer
        {
            get { return _initializer; }
            set { _initializer = value; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _initializer, prefix, postfix);
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as AnonymousObjectCreationExpression;
            if (clone == null)
                return;

            clone._initializer = Clone(cloneContext, _initializer);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            _initializer.Resolve(parseContext);
            
            var anonymousType = TypeManager.GetAnonymousType(_initializer);

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
                DumpChild(Initializer, sw);
        }
    }
}