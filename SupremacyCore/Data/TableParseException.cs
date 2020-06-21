// TableParseException.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Data
{
    [Serializable]
    public sealed class TableParseException : Exception
    {
        private const string baseMessage =
            "An error occurred while parsing a table";
        private const string nameMessage =
            "An error occurred while parsing table ";

        public string Details { get; }

        public TableParseException()
            : base(baseMessage)
        {
            Details = null;
        }

        public TableParseException(string details)
            : base(baseMessage)
        {
            Details = details;
        }

        public TableParseException(string tableName, string details)
            : base(nameMessage + tableName)
        {
            Details = details;
        }

        public TableParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
