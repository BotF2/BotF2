// NetException.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.Serialization;

namespace Supremacy.Network
{
    [Serializable]
    public sealed class NetException : Exception
    {
        public NetException()
        { }
        public NetException(string message) : base(message) { }
        private NetException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public NetException(string message, Exception innerException) : base(message, innerException) { }
    }
}
