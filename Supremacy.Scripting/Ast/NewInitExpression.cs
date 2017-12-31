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
                throw new ArgumentNullException("type");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (initializer == null)
                throw new ArgumentNullException("initializer");

            _initializer = initializer;
        }

        public NewInitExpression([NotNull] Expression type, [NotNull] Arguments arguments, [NotNull] IEnumerable<NewInitMemberBinding> memberBindings, SourceSpan location)
            : base(type, arguments, location)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (memberBindings == null)
                throw new ArgumentNullException("memberBindings");

            _memberBindings = memberBindings;
        }

        protected override bool HasInitializer
        {
            get { return ((_initializer != null) || (_memberBindings != null)); }
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            if (ExpressionClass != ExpressionClass.Invalid)
                return this;

            var baseResolved = base.DoResolve(parseContext);

            if (Type == null)
                return null;

            var previous = parseContext.CurrentInitializerVariable;

            parseContext.CurrentInitializerVariable = new InitializerTargetExpression(this);

            if (_initializer != null)
            {
                _initializer.Resolve(parseContext);
            }
            else
            {
                foreach (var memberBinding in _memberBindings)
                    memberBinding.DoResolve(parseContext);
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
                var elementInitializers = _initializer.Initializers
                    .Cast<CollectionElementInitializer>()
                    .Select(o => o.TransformInitializer(generator));

                return MSAst::Expression.ListInit(
                    (MSAst::NewExpression)base.TransformCore(generator),
                    elementInitializers);
            }

            var memberBindings = _initializer.Initializers
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
                foreach (var memberBinding in _memberBindings)
                {
                    if (i++ != 0)
                        sw.Write(", ");
                    DumpChild(memberBinding, sw, indentChange);
                }

                sw.Write(_memberBindings.Any() ? " }" : "}");
            }
        }

        #region Dependent Type: InitializerTargetExpression
        private sealed class InitializerTargetExpression : Expression
        {
            private readonly NewInitExpression _newInstance;

            public InitializerTargetExpression([NotNull] NewInitExpression newInstance)
            {
                if (newInstance == null)
                    throw new ArgumentNullException("newInstance");

                _newInstance = newInstance;

                Type = newInstance.Type;
                ExpressionClass = newInstance.ExpressionClass;
                Span = newInstance.Span;
            }

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
        private readonly MemberInfo _member;
        private Expression _initializer;

        public NewInitMemberBinding(MemberInfo member, Expression initializer)
        {
            if (member == null)
                throw new ArgumentNullException("member");
            if (initializer == null)
                throw new ArgumentNullException("initializer");

            _member = member;
            _initializer = initializer;
        }

        public MemberInfo Member
        {
            get { return _member; }
        }

        public Expression Initializer
        {
            get { return _initializer; }
        }

        public MSAst.MemberAssignment MakeAssignment(ScriptGenerator generator)
        {
            return MSAst.Expression.Bind(
                _member,
                _initializer.Transform(generator));
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(Member.Name);
            sw.Write(" = ");
            DumpChild(_initializer, sw, indentChange);                
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            _initializer = _initializer.Resolve(parseContext);
            return this;
        }
    }
}