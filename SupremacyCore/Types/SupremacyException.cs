// SupremacyException.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.Serialization;

namespace Supremacy.Types
{
    public enum SupremacyExceptionAction : byte
    {
        Undefined = 0,
        Continue,
        Disconnect,
        Exit
    }

    [Serializable]
    public class SupremacyException : Exception
    {
        private readonly SupremacyExceptionAction _action;

        public SupremacyExceptionAction Action => _action;

        public SupremacyException()
        {
            _action = SupremacyExceptionAction.Undefined;
        }

        public SupremacyException(string message)
            : base(message)
        {
            _action = SupremacyExceptionAction.Undefined;
        }

        public SupremacyException(string message, SupremacyExceptionAction action)
            : base(message)
        {
            _action = action;
        }

        public SupremacyException(string message, Exception innerException)
            : base(message, innerException)
        {
            _action = SupremacyExceptionAction.Undefined;
        }

        public SupremacyException(string message, Exception innerException, SupremacyExceptionAction action)
            : base(message, innerException)
        {
            _action = action;
        }

        public SupremacyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _action = (SupremacyExceptionAction)info.GetValue("Action", typeof(SupremacyExceptionAction));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Action", _action);
        }
    }
}
