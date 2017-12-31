// XmlHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Xml.Schema;

using Supremacy.Types;

namespace Supremacy.Utility
{
    public static class XmlHelper
    {
        public static void ValidateXml(string fileName, ValidationEventArgs e)
        {
            throw new GameDataException(e.Message, fileName, e.Exception);
        }
    }
}
