// DefaultDispatcherService.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows.Threading;

using Supremacy.Annotations;

namespace Supremacy.Client
{
    public class DefaultDispatcherService : IDispatcherService
    {
        private readonly Dispatcher _dispatcher;

        public DefaultDispatcherService([NotNull] Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException("dispatcher");
        }

        public void Invoke(Delegate target, params object[] args)
        {
            _ = _dispatcher.Invoke(target, args);
        }

        public void InvokeAsync(Delegate target, params object[] args)
        {
            _ = _dispatcher.BeginInvoke(target, args);
        }
    }
}