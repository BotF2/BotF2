using System;
using System.Dynamic;

namespace Supremacy.Scripting.Runtime.Binders
{
    sealed class SxeInvokeBinder : InvokeBinder
    {
        public SxeInvokeBinder(BinderState state, CallInfo callInfo)
            : base(callInfo)
        {
            if (callInfo == null)
            {
                throw new ArgumentNullException("callInfo");
            }

            Binder = state ?? throw new ArgumentNullException("state");
        }

        public override int GetHashCode()
        {
            return 197 ^ base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is SxeInvokeBinder && base.Equals(obj);
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError)
        {
            return new DynamicMetaObject(
                System.Linq.Expressions.Expression.Invoke(
                target.Expression,
                args[0].Expression),
                target.Restrictions.Merge(args[0].Restrictions));
        }

        #region Implementation of ISxeSite
        public BinderState Binder { get; }
        #endregion
    }
}