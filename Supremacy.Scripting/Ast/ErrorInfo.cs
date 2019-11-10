using System;

using Supremacy.Annotations;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public sealed class ErrorInfo
    {
        private readonly int _code;
        private readonly string _message;
        private readonly Severity _severity;

        public ErrorInfo(int code, [NotNull] string message)
         : this(code, message, Severity.Error) { }

        public ErrorInfo(int code, [NotNull] string message, Severity severity)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            _code = code;
            _severity = severity;
            _message = message;
        }

        public int Code
        {
            get { return _code; }
        }

        public string Message
        {
            get { return _message; }
        }

        public Severity Severity
        {
            get { return _severity; }
        }

        public bool IsError
        {
            get { return (Severity >= Severity.Error); }
        }

        public string FormatMessage(params object[] args)
        {
            return string.Format(_message, args);
        }
    }
}