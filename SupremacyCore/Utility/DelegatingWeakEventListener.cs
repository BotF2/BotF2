// DelegatingWeakEventListener.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Annotations;

namespace Supremacy.Utility
{
    public sealed class DelegatingWeakEventListener
        : DelegatingWeakEventListener<EventHandler>
    {
        public DelegatingWeakEventListener([NotNull] EventHandler handler)
            : base(handler) {}
    }
}