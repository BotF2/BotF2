// Copyright (c) 2007, Paul Stovell
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions are 
// met:
//
//     * Redistributions of source code must retain the above copyright 
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright 
//       notice, this list of conditions and the following disclaimer in 
//       the documentation and/or other materials provided with the 
//       distribution.
//     * Neither the name of Paul Stovell nor the names of its contributors 
//       may be used to endorse or promote products derived from this 
//       software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;

namespace Supremacy.Types
{
    /// <summary>
    /// Empty delegate used when a StateScope has been entered or left. 
    /// </summary>
    public delegate void StateScopeChangedCallback();

    /// <summary>
    /// This class is used to suppress events and to temporarily set property values. It is necessary 
    /// because when suppressing things like events using simple boolean flags, if one thread 
    /// suppresses it, then another suppresses, the first will then release while the other is still 
    /// running - leading to some inconsistent runtime behavior. 
    /// </summary>
    /// <remarks>
    /// <example>
    /// private StateScope _collectionChangedSuspension;
    /// 
    /// using (_collectionChangedSuspension.Enter()) 
    /// {
    ///     // Do stuff
    /// } // Will "Leave()" automatically
    /// 
    /// StateScope isLoadingState = _loadingState.Enter();
    /// // Do stuff
    /// isLoadingState.Leave();
    /// </example>
    /// </remarks>
    public sealed class StateScope : IDisposable
    {
        #region Fields
        private readonly object _stateMaskLock = new object();
        private readonly StateScopeChangedCallback _callback;
        private readonly List<StateScope> _children = new List<StateScope>();
        private readonly StateScope _parent;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StateScope"/> class.
        /// </summary>
        public StateScope() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="StateScope"/> class.
        /// </summary>
        /// <param name="callback">A callback called when the state's IsWithin property changes.</param>
        public StateScope(StateScopeChangedCallback callback)
        {
            _callback = callback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateScope"/> class.
        /// </summary>
        /// <param name="parent">The parent StateScope.</param>
        private StateScope(StateScope parent)
        {
            _parent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether anyone is currently within this state scope.
        /// </summary>
        public bool IsWithin
        {
            get
            {
                lock (_stateMaskLock)
                {
                    return _children.Count > 0;
                }
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            lock (_stateMaskLock)
            {
                if (_parent != null)
                {
                    _parent.ChildDisposed(this);
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enters this state scope.
        /// </summary>
        public StateScope Enter()
        {
            StateScope result = new StateScope(this);
            lock (_stateMaskLock)
            {
                bool wasWithin = IsWithin;
                _children.Add(result);
                if (wasWithin != IsWithin && _callback != null)
                {
                    _callback();
                }
            }
            return result;
        }

        /// <summary>
        /// Leaves this state scope.
        /// </summary>
        public void Leave()
        {
            ((IDisposable)this).Dispose();
        }

        private void ChildDisposed(StateScope child)
        {
            lock (_stateMaskLock)
            {
                bool wasWithin = IsWithin;
                if (_children.Contains(child))
                {
                    _children.Remove(child);
                }
                if (wasWithin != IsWithin && _callback != null)
                {
                    _callback();
                }
            }
        }
        #endregion
    }
}