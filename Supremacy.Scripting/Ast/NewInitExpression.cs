using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions;

using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class NewInitExpression : NewExpression
    {
        private readonly ObjectInitializerExpression _initializer;
        private readonly IEnumerable<NewInitMemberBinding> _memberBindings;

        public NewInitExpression([NotNull] Expression type, [NotNull] Arguments arguments, [NotNull] ObjectInitializerExpression initializer, SourceSpan location)
            : base(type, arguments, location)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            _initializer = initializer ?? throw new ArgumentNullException("initializer");
        }

        public NewInitExpression([NotNull] Expression type, [NotNull] Arguments arguments, [NotNull] IEnumerable<NewInitMemberBinding> memberBindings, SourceSpan location)
            : base(type, arguments, location)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            _memberBindings = memberBindings ?? throw new ArgumentNullException("memberBindings");
        }

        protected override bool HasInitializer => (_initializer != null) || (_memberBindings != null);

        public override Expression DoResolve(ParseContext parseContext)
        {
            if (ExpressionClass != ExpressionClass.Invalid)
            {
                return this;
            }

            Expression baseResolved = base.DoResolve(parseContext);

            if (Type == null)
            {
                return null;
            }

            Expression previous = parseContext.CurrentInitializerVariable;

            parseContext.CurrentInitializerVariable = new InitializerTargetExpression(this);

            if (_initializer != null)
            {
                _ = _initializer.Resolve(parseContext);
            }
            else
            {
                foreach (NewInitMemberBinding memberBinding in _memberBindings)
                {
                    _ = memberBinding.DoResolve(parseContext);
                }
            }

            parseContext.CurrentInitializerVariable = previous;

            return baseResolved;
        }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            if (_memberBindings != null)
            {
                return MSAst.Expression.MemberInit(
                    (MSAst::NewExpression)base.TransformCore(generator),
                    _memberBindings.Select(o => o.MakeAssignment(generator)));
            }

            if (_initializer is CollectionInitializerExpression)
            {
                IEnumerable<MSAst.ElementInit> elementInitializers = _initializer.Initializers
                    .Cast<CollectionElementInitializer>()
                    .Select(o => o.TransformInitializer(generator));

                return MSAst::Expression.ListInit(
                    (MSAst::NewExpression)base.TransformCore(generator),
                    elementInitializers);
            }

            IEnumerable<MSAst.MemberBinding> memberBindings = _initializer.Initializers
                .Cast<ElementInitializer>()
                .Select(o => o.TransformMemberBinding(generator));

            return MSAst::Expression.MemberInit(
                (MSAst::NewExpression)base.TransformCore(generator),
                memberBindings);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            base.Dump(sw, indentChange);

            if (_initializer != null)
            {
                DumpChild(_initializer, sw, indentChange);
            }
            else
            {
                sw.Write(" { ");

                int i = 0;
                foreach (NewInitMemberBinding memberBinding in _memberBindings)
                {
                    if (i++ != 0)
                    {
                        sw.Write(", ");
                    }

                    DumpChild(memberBinding, sw, indentChange);
                }

                sw.Write(_memberBindings.Any() ? " }" : "}");
            }
        }

        #region Dependent Type: InitializerTargetExpression
        private sealed class InitializerTargetExpression : Expression
        {
            public InitializerTargetExpression([NotNull] NewInitExpression newInstance)
            {
                NewInstance = newInstance ?? throw new ArgumentNullException("newInstance");

                Type = newInstance.Type;
                ExpressionClass = newInstance.ExpressionClass;
                Span = newInstance.Span;
            }

            public NewInitExpression NewInstance { get; }

            public override Expression DoResolve(ParseContext ec)
            {
                return this;
            }

            public override Expression DoResolveLValue(ParseContext ec, Expression rightSide)
            {
                return this;
            }
        }
        #endregion
    }

    public class NewInitMemberBinding : Expression
    {
        public NewInitMemberBinding(MemberInfo member, Expression initializer)
        {
            Member = member ?? throw new ArgumentNullException("member");
            Initializer = initializer ?? throw new ArgumentNullException("initializer");
        }

        public MemberInfo Member { get; }

        public Expression Initializer { get; private set; }

        public MSAst.MemberAssignment MakeAssignment(ScriptGenerator generator)
        {
            return MSAst.Expression.Bind(
                Member,
                Initializer.Transform(generator));
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(Member.Name);
            sw.Write(" = ");
            DumpChild(Initializer, sw, indentChange);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            Initializer = Initializer.Resolve(parseContext);
            return this;
        }
    }
}