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
        private readonly BinderState _binderState;
        private readonly OverloadResolverFactory _resolverFactory;

        public SxeConvertBinder(BinderState binderState, Type type, ConversionResultKind resultKind, OverloadResolverFactory resolverFactory)
            : base(type, (resultKind == ConversionResultKind.ExplicitCast))
        {
            _binderState = binderState;
            _resolverFactory = resolverFactory;
        }

        public OverloadResolverFactory ResolverFactory
        {
            get { return _resolverFactory; }
        }

        public BinderState BinderState
        {
            get { return _binderState; }
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            var restricted = target.Restrict(target.RuntimeType);
            return new DynamicMetaObject(
                Expression.Convert(restricted.Expression, Type),
                restricted.Restrictions);
        }
    }
}