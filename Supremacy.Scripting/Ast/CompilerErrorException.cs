using System;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    internal class CompilerErrorException : Exception
    {
        private readonly ErrorInfo _errorInfo;
        private readonly object[] _messageArgs;

        public int ErrorCode => _errorInfo.Code;

        public Severity ErrorSeverity => _errorInfo.Severity;

        public string ErrorMessage => _errorInfo.FormatMessage(_messageArgs);

        internal CompilerErrorException(ErrorInfo errorInfo, params object[] messageArgs)
        {
            _errorInfo = errorInfo;
            _messageArgs = messageArgs;
        }
    }
}