// IDispatcherService.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Client
{
    public interface IDispatcherService
    {
        void Invoke(Delegate target, params object[] args);
        void InvokeAsync(Delegate target, params object[] args);
    }
}