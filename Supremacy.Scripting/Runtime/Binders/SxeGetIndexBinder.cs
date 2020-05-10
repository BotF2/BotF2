using System;
using System.Dynamic;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal class SxeGetIndexBinder : GetIndexBinder
    {
        public SxeGetIndexBinder(BinderState binderState, CallInfo callInfo)
            : base(callInfo)
        {
            BinderState = binderState;
        }

        public BinderState BinderState { get; }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}