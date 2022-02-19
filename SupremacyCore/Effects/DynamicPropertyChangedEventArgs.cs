// DynamicPropertyChangedEventArgs.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;

namespace Supremacy.Effects
{
    public class DynamicPropertyChangedEventArgs<TValue> : EventArgs
    {
        internal DynamicPropertyChangedEventArgs([NotNull] DynamicProperty<TValue> property, TValue oldValue, TValue newValue)
        {
            Property = property ?? throw new ArgumentNullException("property");
            OldValue = oldValue;
            NewValue = newValue;
        }

        public DynamicProperty<TValue> Property { get; private set; }
        public TValue OldValue { get; private set; }
        public TValue NewValue { get; private set; }
    }
}