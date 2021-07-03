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
using Supremacy.Game;

namespace Supremacy.Client.Views
{
    public class PresentationModelBase : IPresentationModel
    {
        public PresentationModelBase([NotNull] IAppContext appContext)
        {
            AppContext = appContext ?? throw new ArgumentNullException("appContext");
        }

        protected virtual void OnLoaded() { }
        protected virtual void OnUnloaded() { }

        #region Implementation of IPresentationModel

        public IAppContext AppContext { get; }

        public string TurnNumberText => "Turn " + GameContext.Current.TurnNumber;

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