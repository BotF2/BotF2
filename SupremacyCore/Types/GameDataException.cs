// GameDataException.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Types
{
    public class GameDataException : SupremacyException
    {
        private readonly string _fileName;

        public GameDataException(string message, string fileName) : base(message, SupremacyExceptionAction.Disconnect)
        {
            _fileName = fileName;
        }

        public GameDataException(string message, string fileName, Exception innerException) : base(message, innerException, SupremacyExceptionAction.Disconnect)
        {
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }
    }
}