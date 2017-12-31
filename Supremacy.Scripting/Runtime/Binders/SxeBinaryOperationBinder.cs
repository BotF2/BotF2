using System;
using System.Dynamic;

using System.Linq.Expressions;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal class SxeBinaryOperationBinder : BinaryOperationBinder
    {
        private readonly BinderState _binderState;

        public SxeBinaryOperationBinder(BinderState binderState, ExpressionType operation)
            : base(operation)
        {
            if (binderState == null)
                throw new ArgumentNullException("binderState");
            _binderState = binderState;
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}