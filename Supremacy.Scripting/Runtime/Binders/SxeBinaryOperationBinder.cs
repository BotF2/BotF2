using System;
using System.Dynamic;

using System.Linq.Expressions;

namespace Supremacy.Scripting.Runtime.Binders
{
    internal class SxeBinaryOperationBinder : BinaryOperationBinder
    {
        public SxeBinaryOperationBinder(BinderState binderState, ExpressionType operation)
            : base(operation)
        {
            BinderState = binderState ?? throw new ArgumentNullException("binderState");
        }

        public BinderState BinderState { get; }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}