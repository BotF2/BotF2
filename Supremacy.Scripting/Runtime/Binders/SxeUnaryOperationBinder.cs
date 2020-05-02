using System;
using System.Dynamic;

using System.Linq.Expressions;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal class SxeUnaryOperationBinder : UnaryOperationBinder
    {
        public SxeUnaryOperationBinder(BinderState binderState, ExpressionType operation)
            : base(operation)
        {
            BinderState = binderState ?? throw new ArgumentNullException("binderState");
        }

        public BinderState BinderState { get; }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}