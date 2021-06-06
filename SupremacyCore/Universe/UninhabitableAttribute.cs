// UninhabitableAttribute.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Universe
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class UninhabitableAttribute : Attribute
    {
        public static readonly UninhabitableAttribute Default = new UninhabitableAttribute();

        public override bool IsDefaultAttribute()
        {
            return true;
        }

        public override bool Match(object obj)
        {
            return (obj is UninhabitableAttribute);
        }
    }
}
