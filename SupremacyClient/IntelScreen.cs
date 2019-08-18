// AffairsScreen.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Views;
using System;
using System.Windows;

namespace Supremacy.Client
{
    public sealed class IntelScreen : GameScreen<IntelScreenPresentationModel>, IIntelScreenView, IWeakEventListener
    {
        public IntelScreen([NotNull] IUnityContainer container) : base(container) {}

        public override void RefreshScreen()
        {
            base.RefreshScreen();
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return true;
        }
    }
}
