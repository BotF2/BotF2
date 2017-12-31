using System;

using Supremacy.Annotations;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class EnumConstantExpression : ConstantExpression
    {
        private Type _enumType;
        private ConstantExpression _child;

        public EnumConstantExpression([NotNull] ConstantExpression child, Type enumType)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            _child = child;
            _enumType = enumType;

            Type = _child.Type;
        }

        internal EnumConstantExpression()
        {
            // For cloning purposes only.
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as EnumConstantExpression;
            if (clone == null)
                return;

            clone._child = Clone(cloneContext, _child);
            clone._enumType = _enumType;
        }

        public ConstantExpression Child
        {
            get { return _child; }
        }

        public override bool IsZeroInteger
        {
            get { return _child.IsZeroInteger; }
        }

        public override object Value
        {
            get { return Enum.ToObject(_enumType, _child.Value); }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _child, prefix, postfix);

            Type = _child.Type;
        }

        public override ConstantExpression ConvertExplicitly(bool inCheckedContext, Type targetType)
        {
            if (_child.Type == targetType)
                return _child;

            return _child.ConvertExplicitly(inCheckedContext, targetType);
        }

        public override ConstantExpression ConvertImplicitly(Type type)
        {
            var thisType = TypeManager.DropGenericTypeArguments(Type);

            type = TypeManager.DropGenericTypeArguments(type);

            if (thisType == type)
            {
                var childType = TypeManager.DropGenericTypeArguments(_child.Type);
                
                if (type.UnderlyingSystemType != childType)
                    _child = _child.ConvertImplicitly(type.UnderlyingSystemType);

                return this;
            }

            if (!TypeUtils.IsImplicitlyConvertible(Type, type))
                return null;

            return _child.ConvertImplicitly(type);
        }
    }
}