using System;
using System.Dynamic;

using System.Linq.Expressions;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal class SxeUnaryOperationBinder : UnaryOperationBinder
    {
        private readonly BinderState _binderState;

        public SxeUnaryOperationBinder(BinderState binderState, ExpressionType operation)
            : base(operation)
        {
            if (binderState == null)
                throw new ArgumentNullException("binderState");
            _binderState = binderState;
        }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}