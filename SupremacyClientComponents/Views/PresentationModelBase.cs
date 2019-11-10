// PresentationModelBase.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Annotations;
using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    public class PresentationModelBase : IPresentationModel
    {
        private readonly IAppContext _appContext;

        public PresentationModelBase([NotNull] IAppContext appContext)
        {
            if (appContext == null)
                throw new ArgumentNullException("appContext");

            _appContext = appContext;
        }

        protected virtual void OnLoaded() { }
        protected virtual void OnUnloaded() { }

        #region Implementation of IPresentationModel

        public IAppContext AppContext
        {
            get { return _appContext; }
        }

        public void NotifyLoaded()
        {
            OnLoaded();
        }

        public void NotifyUnloaded()
        {
            OnUnloaded();
        }

        #endregion
    }
}