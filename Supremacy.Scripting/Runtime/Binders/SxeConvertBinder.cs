using System;
using System.Dynamic;
using System.Linq.Expressions;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal class SxeConvertBinder : ConvertBinder
    {
        public SxeConvertBinder(BinderState binderState, Type type, ConversionResultKind resultKind, OverloadResolverFactory resolverFactory)
            : base(type, resultKind == ConversionResultKind.ExplicitCast)
        {
            BinderState = binderState;
            ResolverFactory = resolverFactory;
        }

        public OverloadResolverFactory ResolverFactory { get; }

        public BinderState BinderState { get; }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            DynamicMetaObject restricted = target.Restrict(target.RuntimeType);
            return new DynamicMetaObject(
                Expression.Convert(restricted.Expression, Type),
                restricted.Restrictions);
        }
    }
}