using System;

using Supremacy.Annotations;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public sealed class ErrorInfo
    {
        public ErrorInfo(int code, [NotNull] string message)
         : this(code, message, Severity.Error) { }

        public ErrorInfo(int code, [NotNull] string message, Severity severity)
        {
            Code = code;
            Severity = severity;
            Message = message ?? throw new ArgumentNullException("message");
        }

        public int Code { get; }

        public string Message { get; }

        public Severity Severity { get; }

        public bool IsError => Severity >= Severity.Error;

        public string FormatMessage(params object[] args)
        {
            return string.Format(Message, args);
        }
    }
}