using System;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    internal class CompilerErrorException : Exception
    {
        private readonly ErrorInfo _errorInfo;
        private readonly object[] _messageArgs;

        public int ErrorCode
        {
            get { return _errorInfo.Code; }
        }

        public Severity ErrorSeverity
        {
            get { return _errorInfo.Severity; }
        }

        public string ErrorMessage
        {
            get { return _errorInfo.FormatMessage(_messageArgs); }
        }

        internal CompilerErrorException(ErrorInfo errorInfo, params object[] messageArgs)
        {
            _errorInfo = errorInfo;
            _messageArgs = messageArgs;
        }
    }
}