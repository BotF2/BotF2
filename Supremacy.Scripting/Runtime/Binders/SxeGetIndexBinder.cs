using System;
using System.Dynamic;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal class SxeGetIndexBinder : GetIndexBinder
    {
        private readonly BinderState _binderState;

        public SxeGetIndexBinder(BinderState binderState, CallInfo callInfo)
            : base(callInfo)
        {
            _binderState = binderState;
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}