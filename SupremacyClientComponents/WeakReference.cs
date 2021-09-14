// WeakReference.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;

namespace Supremacy.Client
{
    public class WeakReference<T>
    {
        private readonly WeakReference _innerReference;

        public WeakReference(T target)
        {
            _innerReference = new WeakReference(target);
        }

        public bool IsAlive => _innerReference.IsAlive;

        public T Target => (T)_innerReference.Target;

        public static implicit operator T([NotNull] WeakReference<T> weakReference)
        {
            if (weakReference == null)
            {
                throw new ArgumentNullException("weakReference");
            }

            return weakReference.Target;
        }
    }
}