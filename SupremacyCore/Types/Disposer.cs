// Disposer.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;

namespace Supremacy.Types
{
    public sealed class Disposer : IDisposable
    {
        private bool _isDisposed;
        private readonly Action _disposeAction;

        public Disposer([NotNull] Action disposeAction)
        {
            _disposeAction = disposeAction ?? throw new ArgumentNullException("disposeAction");
        }

        ~Disposer()
        {
            Dispose();
        }

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            try
            {
                _disposeAction();
            }
            finally
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
        #endregion
    }
}