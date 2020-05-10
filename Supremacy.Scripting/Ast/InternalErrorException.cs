using System;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public class InternalErrorException : Exception
    {
        public InternalErrorException()
            : base("Internal error")
        {
        }

        public InternalErrorException(string message)
            : base(message)
        {
        }

        public InternalErrorException(string message, params object[] args)
            : base(String.Format(message, args))
        { }

        public InternalErrorException(Exception e, SourceSpan location)
            : base(string.Format("Internal error at location '{0}'.", location), e)
        {
        }

        public InternalErrorException(Exception e, Expression source)
            : base(string.Format("Internal error at location '{0}': {1}.", source.Span, source.GetSignatureForError()), e)
        {
        }
    }
}