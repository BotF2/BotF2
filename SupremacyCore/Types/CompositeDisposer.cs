// CompositeDisposer.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Annotations;

namespace Supremacy.Types
{
    public sealed class CompositeDisposer : IDisposable
    {
        private readonly List<IDisposable> _children;
        private bool _isDisposed;

        public CompositeDisposer()
        {
            _children = new List<IDisposable>();
        }

        public CompositeDisposer(IEnumerable<IDisposable> initialChildren)
        {
            _children = new List<IDisposable>(initialChildren);
        }

        ~CompositeDisposer()
        {
            Dispose();
        }

        public void AddChild([NotNull] IDisposable child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            if (_isDisposed)
            {
                throw new ObjectDisposedException(
                    "Cannot add children after a CompositeDisposer has been disposed.");
            }

            _children.Add(child);
        }

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                while (_children.Count > 0)
                {
                    _children[0].Dispose();
                    _children.RemoveAt(0);
                }
            }
            finally
            {
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}