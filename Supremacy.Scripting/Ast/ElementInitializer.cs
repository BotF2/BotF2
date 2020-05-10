using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions;

using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class ElementInitializer : Expression
    {
        private Expression _value;
        private MemberExpression _target;

        public string MemberName { get; set; }

        public Expression Value
        {
            get => _value;
            set => _value = value;
        }

        public ElementInitializer() { }

        public ElementInitializer(string name, Expression value, SourceSpan span)
        {
            MemberName = name;
            Value = value;
            Span = span;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _value, prefix, postfix);
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is ElementInitializer clone))
            {
                return;
            }

            clone._value = Clone(cloneContext, _value);
            clone._target = Clone(cloneContext, _target);
        }

        public MSAst::MemberBinding TransformMemberBinding(ScriptGenerator generator)
        {
            MemberInfo memberInfo = (_target is PropertyExpression)
                                     ? (MemberInfo)((PropertyExpression)_target).PropertyInfo
                                     : ((FieldExpression)_target).FieldInfo;

            if (_value is CollectionInitializerExpression collectionInitializer)
            {
                System.Collections.Generic.IEnumerable<MSAst.ElementInit> elementInitializers = collectionInitializer.Initializers
                    .Cast<CollectionElementInitializer>()
                    .Select(o => o.TransformInitializer(generator));

                return MSAst::Expression.ListBind(
                    memberInfo,
                    elementInitializers);
            }

            return MSAst::Expression.Bind(
                memberInfo,
                _value.Transform(generator));
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if (_value == null)
            {
                return null;
            }


            if (!(MemberLookupFinal(
                         ec,
                         ec.CurrentInitializerVariable.Type,
                         ec.CurrentInitializerVariable.Type,
                         MemberName,
                         MemberTypes.Field | MemberTypes.Property,
                         BindingFlags.Public | BindingFlags.Instance,
                         Span) is MemberExpression me))
            {
                return null;
            }

            _target = me;
            me.InstanceExpression = ec.CurrentInitializerVariable;

            if (_value is ObjectInitializerExpression)
            {
                Expression previous = ec.CurrentInitializerVariable;
                ec.CurrentInitializerVariable = _value;

                _value = _value.Resolve(ec);

                ec.CurrentInitializerVariable = previous;

                if (_value == null)
                {
                    return null;
                }

                Type = _value.Type;
                ExpressionClass = _value.ExpressionClass;

                return this;
            }

            return this;
        }
    }
}