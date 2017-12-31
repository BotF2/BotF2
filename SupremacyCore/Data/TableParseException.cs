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

        private string details;

        public string Details
        {
            get { return details; }
        }

        public TableParseException()
            : base(baseMessage)
        {
            details = null;
        }

        public TableParseException(string details)
            : base(baseMessage)
        {
            this.details = details;
        }

        public TableParseException(string tableName, string details)
            : base(nameMessage + tableName)
        {
            this.details = details;
        }
    }
}
