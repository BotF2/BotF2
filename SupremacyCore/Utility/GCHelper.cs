// GCHelper.cs
// 
// Copyright (c) 2012 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.Reflection;
using System.Windows;

namespace Supremacy.Utility
{
    public static class GCHelper
    {
        private static readonly Func<bool> _purgeWeakEventHandlers;

        static GCHelper()
        {
            Type weakEventTableClass = typeof(WeakEventManager).Assembly.GetType("MS.Internal.WeakEventTable");
            if (weakEventTableClass == null)
                return;

            MethodInfo cleanupMethod = weakEventTableClass.GetMethod("Cleanup", BindingFlags.NonPublic | BindingFlags.Static);
            if (cleanupMethod == null)
                return;

            _purgeWeakEventHandlers = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), cleanupMethod);
        }

        public static void Collect()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            if (_purgeWeakEventHandlers == null)
                return;

            _purgeWeakEventHandlers();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
    }
}