using System;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Runtime.Binders
{
    public static class SxeBinders
    {
        public static MSAst Convert(BinderState binder, Type type, ConversionResultKind resultKind, MSAst target, OverloadResolverFactory resolverFactory)
        {
            return MSAst.Dynamic(
                binder.Convert(type, resultKind, resolverFactory),
                type,
                target);
        }
    }
}